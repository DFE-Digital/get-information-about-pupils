using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Application.Adapters;

/// <summary>
/// Represents the parameters required to execute a search query within the Further Education search service.
/// Includes search keywords, field targets, facet specifications, and optional filters to tailor the query.
/// </summary>
public sealed class SearchServiceAdapterRequest
{
    /// <summary>
    /// The keyword or phrase used to perform the search.
    /// </summary>
    public string SearchKeyword { get; }

    /// <summary>
    /// The number of search results to skip. Default is zero, indicating that no records will be omitted from the beginning of the result set.
    /// </summary>
    public int Offset { get; } = 0;

    /// <summary>
    /// The specific fields within the underlying data source that should be queried.
    /// </summary>
    public IList<string> SearchFields { get; }

    /// <summary>
    /// The set of facet fields to apply to the search query for grouped filtering or aggregation.
    /// </summary>
    public IList<string> Facets { get; }

    /// <summary>
    /// An optional list of filter conditions, where each entry maps a filter name to a set of allowed values.
    /// </summary>
    public IList<FilterRequest> SearchFilterRequests { get; }

    /// <summary>
    /// Gets the configured <see cref="SortOrder"/> instance representing the field and direction
    /// used to order search results. This is typically composed during query setup and injected
    /// into the Azure Search <see cref="SearchOptions.OrderBy"/> clause.
    /// </summary>
    public SortOrder SortOrdering { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchServiceAdapterRequest"/> class,
    /// ensuring all required fields are populated and immutable for consistent query execution.
    /// </summary>
    /// <param name="searchKeyword">The keyword or phrase to match against any given search fields.</param>
    /// <param name="searchFields">The list of fields to include in the keyword-based search operation.</param>
    /// <param name="facets">A list of facet keys for grouping and filtering results.</param>
    /// <param name="searchFilterRequests">Optional filters for refining the search query, keyed by filter name.</param>
    /// <param name="offset">Specifies how many results to skip in the returned dataset. Defaults to zero.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="searchKeyword"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="searchFields"/> or <paramref name="facets"/> is null or empty.</exception>
    public SearchServiceAdapterRequest(
        string searchKeyword,
        IList<string> searchFields,
        SortOrder sortOrdering,
        IList<string>? facets = null,
        IList<FilterRequest>? searchFilterRequests = null,
        int offset = 0)
    {
        SearchKeyword = !string.IsNullOrWhiteSpace(searchKeyword)
            ? searchKeyword
            : throw new ArgumentException(
                $"{nameof(searchKeyword)} cannot be null or whitespace.", nameof(searchKeyword));

        SearchFields = searchFields?.Count > 0
            ? searchFields
            : throw new ArgumentException(
                $"A valid {nameof(searchFields)} argument must be provided.", nameof(searchFields));

        SortOrdering = sortOrdering;
        Facets = facets ?? [];
        SearchFilterRequests = searchFilterRequests ?? [];
        Offset = offset;
    }

    /// <summary>
    /// Factory method for creating a <see cref="SearchServiceAdapterRequest"/> without
    /// needing to use the constructor directly. Improves readability and usability for consumers of the API.
    /// </summary>
    public static SearchServiceAdapterRequest Create(
        string searchKeyword,
        IList<string> searchFields,
        IList<string> facets,
        SortOrder sortOrdering,
        IList<FilterRequest>? searchFilterRequests = null,
        int offset = 0)
            => new(searchKeyword, searchFields, sortOrdering, facets, searchFilterRequests, offset);
}
