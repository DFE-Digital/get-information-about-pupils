
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.SharedTests.Infrastructure.SearchIndex;
using DfE.GIAP.SharedTests.TestDoubles.Configuration;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Search.SearchByKeyWords;

public class SearchByKeyWordsUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly SearchIndexFixture _searchIndexFixture;

    public SearchByKeyWordsUseCaseIntegrationTests(SearchIndexFixture searchIndexFixture)
    {
        ArgumentNullException.ThrowIfNull(searchIndexFixture);
        _searchIndexFixture = searchIndexFixture;

    }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        IConfiguration searchConfiguration =
            ConfigurationTestDoubles.DefaultConfigurationBuilder()
                .WithSearchIndexOptions()
                .WithAzureSearchOptions()
                .WithAzureSearchConnectionOptions()
                .WithSearchCriteriaOptions()
                .WithFilterKeyToFilterExpressionMapOptions()
                .Build();

        services
            .AddSearchDependencies(searchConfiguration);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task SearchByKeyWordsUseCase_Returns_Results_When_HandleRequest()
    {
        await _searchIndexFixture!.StubAvailableIndexes(["FE_INDEX_NAME"]);

        await _searchIndexFixture.StubIndex(
            indexName: "FE_INDEX_NAME",
            values: AzureFurtherEducationSearchResponseDtoTestDoubles.Generate(count: 30));

        IUseCase<SearchRequest, SearchResponse> sut =
            ResolveApplicationType<IUseCase<SearchRequest, SearchResponse>>()!;

        SortOrder sortOrder = new(
            sortField: "Forename",
            sortDirection: "desc",
            validSortFields: ["Forename", "Surname"]);

        SearchRequest request = new(searchKeywords: "test", sortOrder);

        // act
        SearchResponse response = await sut.HandleRequestAsync(request);

        // assert
        Assert.NotNull(response);
        Assert.NotNull(response.LearnerSearchResults);
        Assert.Equal(SearchResponseStatus.Success, response.Status);
        Assert.Equal(30, response.TotalNumberOfResults);
    }
}
