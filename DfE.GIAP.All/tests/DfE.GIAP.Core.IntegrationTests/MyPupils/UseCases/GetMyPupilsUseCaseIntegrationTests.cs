using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Options;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.IntegrationTests.DataTransferObjects;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Options;
using DfE.GIAP.SharedTests.Features.MyPupils.DataTransferObjects;
using DfE.GIAP.SharedTests.Features.MyPupils.Domain;
using DfE.GIAP.SharedTests.Infrastructure.WireMock;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Request;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Response;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

public sealed class GetMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly GiapCosmosDbFixture _cosmosDbFixture;
    private readonly WireMockServerFixture _searchIndexFixture;
    private const string MyPupilsContainerName = "mypupils";

    // TODO NEED TO SEED COSMOSDB PUPIL-NOSKILL CONTAINER AND REPLACE INDEX CALL

    public GetMyPupilsUseCaseIntegrationTests(GiapCosmosDbFixture cosmosDbFixture, WireMockServerFixture searchIndexFixture)
    {
        ArgumentNullException.ThrowIfNull(cosmosDbFixture);
        _cosmosDbFixture = cosmosDbFixture;

        ArgumentNullException.ThrowIfNull(searchIndexFixture);
        _searchIndexFixture = searchIndexFixture;
    }

    protected override async Task OnInitializeAsync(IServiceCollection services)
    {
        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.ClearDatabaseAsync());

        IConfiguration indexConfiguration =
            ConfigurationTestDoubles.DefaultConfigurationBuilder()
                .WithAzureSearchOptions()
                .WithAzureSearchConnectionOptions()
                .Build();

        services
            .AddOptions<AzureSearchOptions>()
            .Bind(indexConfiguration.GetSection(nameof(AzureSearchOptions)));

        services
            .AddOptions<AzureSearchConnectionOptions>()
            .Bind(indexConfiguration.GetSection(nameof(AzureSearchConnectionOptions)));

        services
            .AddMyPupilsCore();
    }

    [Fact]
    public async Task GetMyPupils_HasPupils_In_MyPupils_Returns_Npd_And_PupilPremium_Pupils()
    {
        HttpMappingRequest request = HttpMappingRequest.Create(
            httpMappingFiles: [
                new HttpMappingFile(
                    key: "npd",
                    fileName: "npd_searchindex_returns_many_pupils.json"),
                new HttpMappingFile(
                    key: "pupil-premium",
                    fileName: "pupilpremium_searchindex_returns_many_pupils.json")
            ]);

        HttpMappedResponses stubbedResponses = await _searchIndexFixture.RegisterHttpMapping(request);

        AzureSearchPostDto npdResponse =
            stubbedResponses
                .GetResponseByKey("npd")
                .GetResponseBody<AzureSearchPostDto>()!;

        AzureSearchPostDto pupilPremiumResponse =
            stubbedResponses
                .GetResponseByKey("pupil-premium")
                .GetResponseBody<AzureSearchPostDto>()!;

        List<UniquePupilNumber> allPupilUpns = npdResponse.value!
            .Select(t => t.UPN)
            .Concat(pupilPremiumResponse.value!.Select(t => t.UPN))
            .Select(t => new UniquePupilNumber(t))
            .ToList();

        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        MyPupilsDocumentDto myPupilsDocument = MyPupilsDocumentDtoTestDoubles.Create(
            myPupilsId,
            upns: UniquePupilNumbers.Create(allPupilUpns));

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.WriteItemAsync(containerName: MyPupilsContainerName, myPupilsDocument));

        // Act
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveApplicationType<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(myPupilsId.Value));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.MyPupils);
        Assert.Equal(35, getMyPupilsResponse.MyPupils.Count);

        MapAzureSearchIndexDtosToPupilDtos mapAzureSearchIndexDtosToPupilDtosMapper = new();

        List<MyPupilsModel> expectedPupils =
            npdResponse.value!
                .Concat(pupilPremiumResponse.value!)
                .Select(mapAzureSearchIndexDtosToPupilDtosMapper.Map!).ToList();

        foreach (MyPupilsModel expectedPupil in expectedPupils)
        {
            MyPupilsModel? actual = getMyPupilsResponse.MyPupils.Values.Single(pupil => pupil.UniquePupilNumber.Equals(expectedPupil.UniquePupilNumber));

            Assert.NotNull(actual);
            Assert.Equal(expectedPupil.Forename, actual.Forename);
            Assert.Equal(expectedPupil.Surname, actual.Surname);
            Assert.Equal(expectedPupil.DateOfBirth, actual.DateOfBirth);
            Assert.Equal(expectedPupil.Sex, actual.Sex);
            Assert.Equal(expectedPupil.LocalAuthorityCode, actual.LocalAuthorityCode);

            bool isPupilPremium = pupilPremiumResponse.value!.Any(t => t!.UPN == expectedPupil.UniquePupilNumber);
            Assert.Equal(isPupilPremium, actual!.IsPupilPremium);
        }
    }

    [Fact]
    public async Task GetMyPupils_NoPupils_Returns_Empty_And_DoesNot_Call_SearchIndexes()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        MyPupilsDocumentDto document =
            MyPupilsDocumentDtoTestDoubles.Create(
                myPupilsId,
                upns: UniquePupilNumbers.Create(uniquePupilNumbers: []));

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.WriteItemAsync(containerName: MyPupilsContainerName, document));

        // Act
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveApplicationType<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(myPupilsId.Value));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.MyPupils);
        Assert.Empty(getMyPupilsResponse.MyPupils.Values);
    }

    private sealed class MapAzureSearchIndexDtosToPupilDtos : IMapper<AzureNpdSearchResponseDto, MyPupilsModel>
    {
        public MyPupilsModel Map(AzureNpdSearchResponseDto input)
        {
            return new()
            {
                UniquePupilNumber = new(input.UPN),
                DateOfBirth = input.DOB ?? string.Empty,
                Forename = input.Forename,
                Surname = input.Surname,
                Sex = input.Sex?.ToString() ?? string.Empty,
                IsPupilPremium = false, // not used when asserting - not mapped from entity, rather that the pupil-premium index was called.
                LocalAuthorityCode = int.Parse(input.LocalAuthority),
            };
        }
    }
}
