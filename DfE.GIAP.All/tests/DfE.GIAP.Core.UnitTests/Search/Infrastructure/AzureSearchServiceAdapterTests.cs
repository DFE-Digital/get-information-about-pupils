using Azure;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Infrastructure;
using DfE.GIAP.Core.Search.Infrastructure.Builders;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Options;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.Options;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure;

public sealed class AzureSearchServiceAdapterTests
{
    private const string _mockSearchIndexKey = "further-education";
    private readonly ISearchByKeywordService _mockSearchByKeywordService;
    private readonly IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners> _mockSearchResultMapper =
        PageableSearchResultsToLearnerResultsMapperTestDouble.DefaultMock();
    private readonly IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets> _mockFacetsMapper =
        FacetResultToLearnerFacetsMapperTestDouble.DefaultMock();
    private readonly AzureSearchOptions _mockAzureSearchOptions = AzureSearchOptionsTestDouble.Stub();
    private readonly ISearchOptionsBuilder _mockSearchOptionsBuilder =
        new SearchOptionsBuilder(searchFilterExpressionsBuilder: FilterExpressionTestDouble.Mock());

    private static AzureSearchServiceAdapter CreateSearchServiceAdapterWith(
        ISearchByKeywordService searchByKeywordService,
        IOptions<AzureSearchOptions> searchOptions,
        IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners> searchResultMapper,
        IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets> facetsMapper,
        ISearchOptionsBuilder searchOptionsBuilder
       ) =>
           new(searchByKeywordService, searchOptions, searchResultMapper, facetsMapper, searchOptionsBuilder);

    private SearchIndexOptions _mockAzureIndexOptions => _mockAzureSearchOptions.Indexes![_mockSearchIndexKey];

    public AzureSearchServiceAdapterTests() =>
        _mockSearchByKeywordService =
            new SearchServiceTestDouble()
                .WithSearchKeywordAndCollection("SearchKeyword", _mockAzureIndexOptions.SearchIndex)
                .WithSearchResults(new SearchResultFakeBuilder().WithSearchResults().Create())
                .Create();

    [Fact]
    public async Task Search_SendsCorrectRequestToSearchService()
    {
        // arrange
        Mock<Response> responseMock = new();
        Response<SearchResults<LearnerDataTransferObject>> searchServiceResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder().WithSearchResults().Create(),
                    10, null, null, responseMock.Object), responseMock.Object);

        Mock<ISearchByKeywordService> mockService =
            SearchByKeywordServiceTestDouble.MockFor(searchServiceResponse);

        SearchServiceAdapterRequest searchServiceAdapterRequest =
            SearchServiceAdapterRequestTestDouble.Stub();

        ISearchServiceAdapter<Learners, SearchFacets> searchServiceAdapter =
            CreateSearchServiceAdapterWith(
                mockService.Object,
                OptionsTestDoubles.MockAs(_mockAzureSearchOptions),
                _mockSearchResultMapper,
                _mockFacetsMapper,
                _mockSearchOptionsBuilder);

        // act
        _ = await searchServiceAdapter.SearchAsync(searchServiceAdapterRequest);

        // assert

        SearchByKeywordServiceTestDouble.keywordPassedToSearchService
            .Should().Be(searchServiceAdapterRequest.SearchKeyword);
        SearchByKeywordServiceTestDouble.indexPassedToSearchService
            .Should().Be(_mockAzureIndexOptions.SearchIndex);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.Size
            .Should().Be(_mockAzureIndexOptions.Size);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.SearchMode
            .Should().Be((SearchMode)_mockAzureIndexOptions.SearchMode);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.IncludeTotalCount
            .Should().Be(_mockAzureIndexOptions.IncludeTotalCount);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.SearchFields
            .Should().BeEquivalentTo(searchServiceAdapterRequest.SearchFields);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.Facets
            .Should().BeEquivalentTo(searchServiceAdapterRequest.Facets);
    }

    [Fact]
    public void Search_WithNullSearchOptions_ThrowsApplicationException()
    {
        // act, assert

        Func<AzureSearchServiceAdapter> act = () =>
        {
            return new(
                _mockSearchByKeywordService,
                OptionsTestDoubles.MockNullOptions<AzureSearchOptions>(),
                _mockSearchResultMapper,
                _mockFacetsMapper,
                _mockSearchOptionsBuilder);
        };

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public Task Search_MapperThrowsException_ExceptionPassesThrough()
    {
        // arrange
        IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners> mockEstablishmentResultsMapper =
            PageableSearchResultsToLearnerResultsMapperTestDouble.MockMapperThrowingArgumentException();

        ISearchServiceAdapter<Learners, SearchFacets> searchServiceAdapter =
            CreateSearchServiceAdapterWith(
                _mockSearchByKeywordService,
                OptionsTestDoubles.MockAs(_mockAzureSearchOptions),
                mockEstablishmentResultsMapper,
                _mockFacetsMapper,
                _mockSearchOptionsBuilder);

        // act, assert.
        return searchServiceAdapter
            .Invoking(adapter =>
                adapter.SearchAsync(new SearchServiceAdapterRequest(
                    searchIndexKey: _mockSearchIndexKey,
                    searchKeyword: "SearchKeyword",
                    searchFields: [],
                    facets: [],
                    sortOrdering: new SortOrder(
                        sortField: "sortField",
                        sortDirection: "sortDirection",
                        validSortFields: []))
                    )
                )
            .Should()
            .ThrowAsync<ArgumentException>();
    }
}
