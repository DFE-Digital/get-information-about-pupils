using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// A test double builder for creating mocked <see cref="Response{T}"/> objects
/// containing <see cref="SearchResults{T}"/> for use in unit tests.
/// This allows tests to simulate Azure Cognitive Search responses without
/// calling the real search service.
/// </summary>
/// <remarks>
/// This builder supports configuring:
/// <list type="bullet">
/// <item>Search results (documents returned by the search)</item>
/// <item>Facet results (aggregations returned by the search)</item>
/// </list>
/// The <see cref="Create"/> method produces a <see cref="Response{T}"/> that
/// can be returned from mocked search client calls.
/// </remarks>
internal class AzureSearchResponseBuilder
{
    /// <summary>
    /// The collection of search results to include in the simulated response.
    /// </summary>
    private IEnumerable<SearchResult<LearnerDataTransferObject>>? _searchResults;

    /// <summary>
    /// The collection of facet results to include in the simulated response.
    /// Keys are facet field names, values are lists of facet counts.
    /// </summary>
    private Dictionary<string, IList<FacetResult>>? _facetResults;

    /// <summary>
    /// Configures the builder with a set of search results to return.
    /// </summary>
    /// <param name="searchResults">
    /// The search results to include in the simulated response.
    /// </param>
    /// <returns>
    /// The current <see cref="AzureSearchResponseBuilder"/> instance for fluent chaining.
    /// </returns>
    public AzureSearchResponseBuilder WithSearchResults(
        IEnumerable<SearchResult<LearnerDataTransferObject>>? searchResults)
    {
        _searchResults = searchResults;
        return this;
    }

    /// <summary>
    /// Configures the builder with a set of facet results to return.
    /// </summary>
    /// <param name="facetResults">
    /// The facet results to include in the simulated response.
    /// </param>
    /// <returns>
    /// The current <see cref="AzureSearchResponseBuilder"/> instance for fluent chaining.
    /// </returns>
    public AzureSearchResponseBuilder WithFacets(
        Dictionary<string, IList<FacetResult>>? facetResults)
    {
        _facetResults = facetResults;
        return this;
    }

    /// <summary>
    /// Creates a simulated <see cref="Response{SearchResults}"/> object containing
    /// the configured search and facet results.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="SearchModelFactory.SearchResults"/> to construct the
    /// <see cref="SearchResults{T}"/> instance, and wraps it in a mocked
    /// <see cref="Response"/> to mimic the Azure SDK's return type.
    /// </remarks>
    /// <returns>
    /// A <see cref="Response{SearchResults}"/> containing the configured test data.
    /// </returns>
    public Response<SearchResults<LearnerDataTransferObject>> Create()
    {
        // Create a mock Response object to satisfy the Azure SDK's Response<T> requirements.
        Mock<Response> responseMock = new();

        // Build the SearchResults<Learner> using the Azure SDK's model factory.
        // Parameters:
        // - searchResults: the documents to return
        // - totalCount: hard-coded to 100 for test purposes
        // - facets: the facet results to include
        // - coverage: null (not used in tests)
        // - rawResponse: the mocked Response object
        return Response.FromValue(
            SearchModelFactory.SearchResults(
                _searchResults,
                100,
                _facetResults,
                null,
                responseMock.Object),
            responseMock.Object);
    }
}
