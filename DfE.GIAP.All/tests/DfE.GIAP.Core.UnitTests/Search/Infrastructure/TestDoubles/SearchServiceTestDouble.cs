using System.Linq.Expressions;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// A fluent builder for creating mocked <see cref="ISearchByKeywordService"/> instances
/// for use in unit tests. This allows tests to simulate Azure Cognitive Search keyword
/// searches without calling the real search service.
/// </summary>
/// <remarks>
/// Supports configuring:
/// <list type="bullet">
/// <item>Search keyword and collection name</item>
/// <item>Search results (documents)</item>
/// <item>Facet results (aggregations)</item>
/// </list>
/// The <see cref="Create"/> method produces a mock service that returns a
/// pre-configured <see cref="Response{SearchResults}"/> when invoked.
/// </remarks>
internal class SearchServiceTestDouble
{
    /// <summary>
    /// Creates a default mock <see cref="ISearchByKeywordService"/> with no configured behavior.
    /// Useful when the service is required as a dependency but not exercised in the test.
    /// </summary>
    public static ISearchByKeywordService DefaultMock() => Mock.Of<ISearchByKeywordService>();

    /// <summary>
    /// Builds an expression representing the expected call to
    /// <see cref="ISearchByKeywordService.SearchAsync{T}"/> for a given keyword and collection.
    /// </summary>
    /// <param name="keyword">The search keyword to match in the setup.</param>
    /// <param name="collection">The collection name to match in the setup.</param>
    /// <returns>
    /// An expression that can be used with Moq's <c>Setup</c> to configure the mock.
    /// </returns>
    public static Expression<Func<
        ISearchByKeywordService,
        Task<Response<SearchResults<LearnerDataTransferObject>>>>> SearchRequest(
        string keyword, string collection) =>
            searchService =>
                searchService.SearchAsync<LearnerDataTransferObject>(
                    keyword, collection, It.IsAny<SearchOptions>());

    // Backing fields for builder configuration
    private string _keyword = string.Empty;
    private string _collection = string.Empty;
    private IEnumerable<SearchResult<LearnerDataTransferObject>>? _searchResults;
    private Dictionary<string, IList<FacetResult>>? _facets;

    /// <summary>
    /// Creates a mock <see cref="ISearchByKeywordService"/> that will return the specified
    /// <paramref name="searchResult"/> when called with the given keyword and collection.
    /// </summary>
    /// <param name="searchResult">The search results to return.</param>
    /// <param name="keyword">The keyword to match in the mock setup.</param>
    /// <param name="collection">The collection name to match in the mock setup.</param>
    /// <returns>A configured mock <see cref="ISearchByKeywordService"/> instance.</returns>
    public ISearchByKeywordService MockFor(
        Response<SearchResults<LearnerDataTransferObject>> searchResult, string keyword, string collection)
    {
        Mock<ISearchByKeywordService> searchServiceMock = new();

        // Configure the mock to return the provided searchResult when called with the expected parameters
        searchServiceMock
            .Setup(SearchRequest(keyword, collection))
            .Returns(Task.FromResult(searchResult))
            .Verifiable(); // Allows later verification that the call was made

        return searchServiceMock.Object;
    }

    /// <summary>
    /// Configures the builder with the search keyword and collection name to expect.
    /// </summary>
    public SearchServiceTestDouble WithSearchKeywordAndCollection(string keyword, string collection)
    {
        _keyword = keyword;
        _collection = collection;
        return this;
    }

    /// <summary>
    /// Configures the builder with the search results to return.
    /// </summary>
    public SearchServiceTestDouble WithSearchResults(IEnumerable<SearchResult<LearnerDataTransferObject>> results)
    {
        _searchResults = results;
        return this;
    }

    /// <summary>
    /// Configures the builder with the facet results to return.
    /// </summary>
    public SearchServiceTestDouble WithFacets(Dictionary<string, IList<FacetResult>> facets)
    {
        _facets = facets;
        return this;
    }

    /// <summary>
    /// Creates the configured mock <see cref="ISearchByKeywordService"/> instance.
    /// </summary>
    /// <remarks>
    /// Internally uses <see cref="AzureSearchResponseBuilder"/> to construct
    /// a <see cref="Response{SearchResults}"/> containing the configured search and facet results.
    /// </remarks>
    public ISearchByKeywordService Create()
    {
        // Build a fake Azure Search response with the configured results and facets
        Response<SearchResults<LearnerDataTransferObject>> response =
            new AzureSearchResponseBuilder()
                .WithSearchResults(_searchResults)
                .WithFacets(_facets)
                .Create();

        // Return a mock service configured to return the fake response
        return MockFor(response, _keyword, _collection);
    }
}
