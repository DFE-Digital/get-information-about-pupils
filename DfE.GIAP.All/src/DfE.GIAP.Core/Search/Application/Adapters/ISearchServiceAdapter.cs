using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Application.Adapters;

/// <summary>
/// Represents a generic interface for adapting search infrastructure to the <c>Dfe.Data.SearchPrototype</c> application.
/// Facilitates compatibility between internal search mechanisms and the expected result schema for prototype-level integrations.
/// </summary>
/// <typeparam name="TSearchResult">
/// Defines the shape of each item returned in the search results.
/// </typeparam>
/// <typeparam name="TFacetResult">
/// Defines the structure of facet groupings associated with the search operation.
/// </typeparam>
public interface ISearchServiceAdapter<TSearchResult, TFacetResult>
{
    /// <summary>
    /// Executes an asynchronous query against the designated search provider using parameters
    /// from the <see cref="SearchServiceAdapterRequest"/> payload.
    /// Returns typed results and optional facet meta-data tailored to the application's domain.
    /// </summary>
    /// <param name="searchServiceAdapterRequest">
    /// Encapsulates the search keyword, field targets, facets, filters, and paging configuration
    /// for the search operation.
    /// </param>
    /// <returns>
    /// A <see cref="SearchResults{TSearchResult, TFacetResult}"/> containing matched items and any associated facets
    /// retrieved from the underlying search engine (e.g., Azure Cognitive Search).
    /// </returns>
    Task<SearchResults<TSearchResult, TFacetResult>> SearchAsync(SearchServiceAdapterRequest searchServiceAdapterRequest);
}

