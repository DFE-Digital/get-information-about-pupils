using DfE.GIAP.Core.IntegrationTests.Fixture.Configuration;
using DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.SharedTests;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Search.SearchByKeyWords;

public class SearchByKeyWordsUseCaseIntegrationTests : BaseIntegrationTest, IClassFixture<ConfigurationFixture>
{
    private ConfigurationFixture ConfigFixture { get; }
    private SearchIndexFixture _mockSearchFixture = null!;

    public SearchByKeyWordsUseCaseIntegrationTests(ConfigurationFixture configurationFixture) : base()
    {
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
            .ConfigureAzureSearchClients()
            .AddSearchDependencies(ConfigFixture.Configuration)
            
            .AddOptions<SearchIndexOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                    configuration
                        .GetSection(nameof(SearchIndexOptions))
                        .Bind(settings));

        return Task.CompletedTask;
    }

    [Fact]
    public async Task SearchByKeyWordsUseCase_Returns_Results_When_HandleRequest()
    {
        IEnumerable<AzureIndexEntity> furtherEducationSearchIndexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 30);
        _mockSearchFixture.StubFurtherEducationSearchIndex(furtherEducationSearchIndexDtos);
        _mockSearchFixture.StubAvailableIndexes(["further-education"]);

        IUseCase <SearchByKeyWordsRequest, SearchByKeyWordsResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<SearchByKeyWordsRequest, SearchByKeyWordsResponse>>()!;

        SortOrder sortOrder = new(sortField: "Forename", sortDirection: "desc", ["Forename","Surname"]);
        SearchByKeyWordsRequest request = new(searchKeywords: "test", sortOrder);

        // act
        SearchByKeyWordsResponse response = await sut.HandleRequestAsync(request);

        // assert
        Assert.NotNull(response);
        Assert.NotNull(response);
        Assert.NotNull(response.LearnerSearchResults);
        Assert.Equal(SearchResponseStatus.Success, response.Status);
        Assert.Equal(30, response.TotalNumberOfResults);

        //_mockSearchFixture = mockSearchFixture;
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
