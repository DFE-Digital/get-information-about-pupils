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
using Microsoft.Extensions.Options;

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
        services
            .AddSharedTestDependencies()
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
        // arrange
        using SearchIndexFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        IEnumerable<AzureIndexEntity> furtherEducationSearchIndexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 30);
        mockSearchFixture.StubFurtherEducationSearchIndex(furtherEducationSearchIndexDtos);
        mockSearchFixture.StubAvailableIndexes(["further-education"]);

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

        _mockSearchFixture = mockSearchFixture;
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
