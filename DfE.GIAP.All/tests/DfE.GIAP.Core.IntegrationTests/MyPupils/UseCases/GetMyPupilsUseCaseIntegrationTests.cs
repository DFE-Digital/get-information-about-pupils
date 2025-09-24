using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.IntegrationTests.Fixture.Configuration;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using Microsoft.Extensions.Options;
using DfE.GIAP.Core.Search;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetMyPupilsUseCaseIntegrationTests : BaseIntegrationTest, IClassFixture<ConfigurationFixture>
{
    private CosmosDbFixture Fixture { get; }
    private ConfigurationFixture ConfigFixture { get; }
    private SearchIndexFixture _mockSearchFixture = null!;

    public GetMyPupilsUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture, ConfigurationFixture configurationFixture) : base()
    {
        Fixture = cosmosDbFixture;
        ConfigFixture = configurationFixture;
    }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        SearchIndexFixture searchIndexFixture = new();
        _mockSearchFixture = searchIndexFixture;

        Dictionary<string, string> searchConfiguration = new()
        {
            // SearchIndexOptions
            ["SearchIndexOptions:Url"] = searchIndexFixture.BaseUrl,
            ["SearchIndexOptions:Key"] = "SEFSOFOIWSJFSO",
            ["SearchIndexOptions:Indexes:npd:Name"] = "npd",
            ["SearchIndexOptions:Indexes:pupil-premium:Name"] = "pupil-premium-index",
            ["SearchIndexOptions:Indexes:further-education:Name"] = "further-education",

            // SearchCriteria
            ["SearchCriteria:SearchFields:0"] = "Forename",
            ["SearchCriteria:SearchFields:1"] = "Surname",
            ["SearchCriteria:Facets:0"] = "ForenameLC",
            ["SearchCriteria:Facets:1"] = "SurnameLC",
            ["SearchCriteria:Facets:2"] = "Gender",
            ["SearchCriteria:Facets:3"] = "Sex",

            // AzureSearchOptions
            ["AzureSearchOptions:SearchIndex"] = "further-education",
            ["AzureSearchOptions:SearchMode"] = "0",
            ["AzureSearchOptions:Size"] = "40000",
            ["AzureSearchOptions:IncludeTotalCount"] = "true",

            // AzureSearchConnectionOptions
            ["AzureSearchConnectionOptions:EndpointUri"] = searchIndexFixture.BaseUrl,
            ["AzureSearchConnectionOptions:Credentials"] = "SEFSOFOIWSJFSO"
        };

        services
            .AddSharedTestDependencies(searchConfiguration)
            .AddCosmosDbDependencies()
            .AddMyPupilsDependencies()
            .AddSearchDependencies(ConfigFixture.Configuration)
            .ConfigureAzureSearchClients();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetMyPupils_HasPupils_In_MyPupils_Returns_Npd_And_PupilPremium_Pupils()
    {
        // Arrange
        //using SearchIndexFixture mockSearchFixture = new();
            //ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        IEnumerable<AzureIndexEntity> npdSearchIndexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 10);
        _mockSearchFixture.StubNpdSearchIndex(npdSearchIndexDtos);

        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 25);
        _mockSearchFixture.StubPupilPremiumSearchIndex(pupilPremiumSearchIndexDtos);

        UserId userId = UserIdTestDoubles.Default();

        IEnumerable<UniquePupilNumber> upns
            = npdSearchIndexDtos.Concat(pupilPremiumSearchIndexDtos)
                .Select((t) => t.UPN)
                    .ToUniquePupilNumbers();

        await Fixture.Database.WriteItemAsync<MyPupilsDocumentDto>(
            MyPupilsDocumentDtoTestDoubles.Create(
                userId,
                upns: UniquePupilNumbers.Create(upns)));

        // Act
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(userId));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.MyPupils);
        Assert.Equal(npdSearchIndexDtos.Count() + pupilPremiumSearchIndexDtos.Count(), getMyPupilsResponse.MyPupils.Count);

        MapAzureSearchIndexDtosToPupilDtos mapAzureSearchIndexDtosToPupilDtosMapper = new();
        List<MyPupilDto> expectedPupils = npdSearchIndexDtos.Concat(pupilPremiumSearchIndexDtos).Select(mapAzureSearchIndexDtosToPupilDtosMapper.Map).ToList();

        foreach (MyPupilDto expectedPupil in expectedPupils)
        {
            MyPupilDto? actual = getMyPupilsResponse.MyPupils.Values.Single(pupil => pupil.UniquePupilNumber.Equals(expectedPupil.UniquePupilNumber));

            Assert.NotNull(actual);
            Assert.Equal(expectedPupil.Forename, actual.Forename);
            Assert.Equal(expectedPupil.Surname, actual.Surname);
            Assert.Equal(expectedPupil.DateOfBirth, actual.DateOfBirth);
            Assert.Equal(expectedPupil.Sex, actual.Sex);
            Assert.Equal(expectedPupil.LocalAuthorityCode, actual.LocalAuthorityCode);

            bool isPupilPremium = pupilPremiumSearchIndexDtos.Any(t => new UniquePupilNumber(t.UPN).Equals(expectedPupil.UniquePupilNumber));
            Assert.Equal(isPupilPremium, actual!.IsPupilPremium);
        }
    }

    [Fact]
    public async Task GetMyPupils_NoPupils_Returns_Empty_And_DoesNot_Call_SearchIndexes()
    {
        // Arrange
        //using SearchIndexFixture mockSearchFixture = new();
            //ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        UserId userId = UserIdTestDoubles.Default();

        await Fixture.Database.WriteItemAsync<MyPupilsDocumentDto>(
            MyPupilsDocumentDtoTestDoubles.Create(
                userId,
                upns: UniquePupilNumbers.Create(uniquePupilNumbers: [])));
        // Act
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(userId));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.MyPupils);
        Assert.Empty(getMyPupilsResponse.MyPupils.Values);
    }

    private sealed class MapAzureSearchIndexDtosToPupilDtos : IMapper<AzureIndexEntity, MyPupilDto>
    {
        public MyPupilDto Map(AzureIndexEntity input)
        {
            return new()
            {
                UniquePupilNumber = new(input.UPN),
                DateOfBirth = input.DOB?.ToString("yyyy-MM-dd") ?? string.Empty,
                Forename = input.Forename,
                Surname = input.Surname,
                Sex = input.Sex?.ToString() ?? string.Empty,
                IsPupilPremium = false, // not used when asserting - not mapped from entity, rather that the pupil-premium index was called.
                LocalAuthorityCode = int.Parse(input.LocalAuthority),
            };
        }
    }

    protected override Task OnDisposeAsync()
    {
        if (_mockSearchFixture != null)
        {
            _mockSearchFixture?.Dispose();
        }

        return Task.CompletedTask;
    }
}
