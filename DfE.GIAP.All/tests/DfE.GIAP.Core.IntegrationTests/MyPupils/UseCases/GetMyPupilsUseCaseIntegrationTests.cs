using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests.Infrastructure.WireMock;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Request;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Response;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

public sealed class GetMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly CosmosDbFixture _cosmosDbFixture;
    private readonly WireMockServerFixture _searchIndexFixture;

    public GetMyPupilsUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture, WireMockServerFixture searchIndexFixture)
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

        services.AddMyPupilsDependencies();
    }

    [Fact]
    public async Task GetMyPupils_HasPupils_In_MyPupils_Returns_Npd_And_PupilPremium_Pupils()
    {
        HttpMappingRequest request = HttpMappingRequest.Create(
            httpMappingFiles: [
                new HttpMappingFile(
                    clientId: "npd",
                    fileName: "contracts/npd_searchindex_returns_many_pupils.json"),
                new HttpMappingFile(
                    clientId: "pupil-premium",
                    fileName: "contracts/pupilpremium_searchindex_returns_many_pupils.json")
            ]);

        HttpMappedResponses stubbedResponses = await _searchIndexFixture.RegisterHttpMapping(request);

        AzureSearchPostDto npdResponse = stubbedResponses.GetResponseById("npd").GetResponseBody<AzureSearchPostDto>()!;
        AzureSearchPostDto pupilPremiumResponse = stubbedResponses.GetResponseById("pupil-premium").GetResponseBody<AzureSearchPostDto>()!;

        List<UniquePupilNumber> allPupilUpns = npdResponse.value!
            .Select(t => t.UPN)
            .Concat(pupilPremiumResponse.value!.Select(t => t.UPN))
            .Select(t => new UniquePupilNumber(t))
            .ToList();

        UserId userId = UserIdTestDoubles.Default();

        MyPupilsDocumentDto myPupilsDocument = MyPupilsDocumentDtoTestDoubles.Create(
            userId,
            upns: UniquePupilNumbers.Create(allPupilUpns));

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.WriteItemAsync(
                containerName: "mypupils", value: myPupilsDocument));

        // Act
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveApplicationType<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(userId));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.MyPupils);
        Assert.Equal(35, getMyPupilsResponse.MyPupils.Count);

        MapAzureSearchIndexDtosToPupilDtos mapAzureSearchIndexDtosToPupilDtosMapper = new();

        List<MyPupilDto> expectedPupils =
            npdResponse.value!
                .Concat(pupilPremiumResponse.value!)
                .Select(mapAzureSearchIndexDtosToPupilDtosMapper.Map!).ToList();

        foreach (MyPupilDto expectedPupil in expectedPupils)
        {
            MyPupilDto? actual = getMyPupilsResponse.MyPupils.Values.Single(pupil => pupil.UniquePupilNumber.Equals(expectedPupil.UniquePupilNumber));

            Assert.NotNull(actual);
            Assert.Equal(expectedPupil.Forename, actual.Forename);
            Assert.Equal(expectedPupil.Surname, actual.Surname);
            Assert.Equal(expectedPupil.DateOfBirth, actual.DateOfBirth);
            Assert.Equal(expectedPupil.Sex, actual.Sex);
            Assert.Equal(expectedPupil.LocalAuthorityCode, actual.LocalAuthorityCode);

            bool isPupilPremium = pupilPremiumResponse.value!.Any(t => new UniquePupilNumber(t!.UPN).Equals(expectedPupil.UniquePupilNumber));
            Assert.Equal(isPupilPremium, actual!.IsPupilPremium);
        }
    }

    [Fact]
    public async Task GetMyPupils_NoPupils_Returns_Empty_And_DoesNot_Call_SearchIndexes()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.WriteItemAsync<MyPupilsDocumentDto>(
                containerName: "mypupils",
                MyPupilsDocumentDtoTestDoubles.Create(userId, upns: UniquePupilNumbers.Create(uniquePupilNumbers: []))));
        // Act
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveApplicationType<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(userId));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.MyPupils);
        Assert.Empty(getMyPupilsResponse.MyPupils.Values);
    }

    private sealed class MapAzureSearchIndexDtosToPupilDtos : IMapper<AzureNpdSearchResponseDto, MyPupilDto>
    {
        public MyPupilDto Map(AzureNpdSearchResponseDto input)
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


public record AzureSearchPostDto
{
    public IEnumerable<AzureNpdSearchResponseDto>? value { get; set; }
}
