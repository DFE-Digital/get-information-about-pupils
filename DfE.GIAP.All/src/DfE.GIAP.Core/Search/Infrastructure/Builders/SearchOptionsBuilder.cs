using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.Search.Infrastructure.Builders;

/// <summary>
/// Provides a concrete implementation of the <see cref="ISearchOptionsBuilder"/> abstraction,
/// which establishes a configured <see cref="SearchOptions"/> instance with prescribed behaviour
/// for Azure Cognitive Search queries.
/// </summary>
public sealed class SearchOptionsBuilder : ISearchOptionsBuilder
{
    private readonly SearchOptions _searchOptions;
    private readonly ISearchFilterExpressionsBuilder? _searchFilterExpressionsBuilder;

    private SearchMode? _searchMode;
    private int? _size;
    private int _offset;
    private bool? _includeTotalCount;
    private IList<string>? _searchFields;
    private IList<string>? _facets;
    private IList<FilterRequest>? _filters;
    private SortOrder? _sortOrder;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchOptionsBuilder"/>, optionally injecting a filter expression builder.
    /// </summary>
    /// <param name="searchFilterExpressionsBuilder">
    /// Optional dependency for building Azure Search-compatible filter expressions from domain-specific filter requests.
    /// </param>
    public SearchOptionsBuilder(ISearchFilterExpressionsBuilder? searchFilterExpressionsBuilder = null)
    {
        _searchFilterExpressionsBuilder = searchFilterExpressionsBuilder;
        _searchOptions = new SearchOptions();
    }

    /// <summary>
    /// Specifies the maximum number of search results to retrieve.
    /// </summary>
    /// <param name="size">Total number of records to return.</param>
    /// <returns>The updated builder instance.</returns>
    public ISearchOptionsBuilder WithSize(int? size)
    {
        _size = size;
        return this;
    }

    /// <summary>
    /// Defines the number of records to skip in the search response (for pagination).
    /// </summary>
    /// <param name="offset">The initial number of results to skip. Defaults to zero.</param>
    /// <returns>The updated builder instance.</returns>
    public ISearchOptionsBuilder WithOffset(int offset = 0)
    {
        _offset = offset;
        return this;
    }

    /// <summary>
    /// Sets the search mode to determine how terms should match (e.g., Any or All).
    /// </summary>
    /// <param name="searchMode">Specifies whether all or any search terms must match.</param>
    /// <returns>The updated builder instance.</returns>
    public ISearchOptionsBuilder WithSearchMode(SearchMode searchMode)
    {
        _searchMode = searchMode;
        return this;
    }

    /// <summary>
    /// Configures the response to include the total count of results.
    /// </summary>
    /// <param name="includeTotalCount">Boolean flag indicating whether total count should be included.</param>
    /// <returns>The updated builder instance.</returns>
    public ISearchOptionsBuilder WithIncludeTotalCount(bool? includeTotalCount)
    {
        _includeTotalCount = includeTotalCount;
        return this;
    }

    /// <summary>
    /// Specifies which fields should be searchable.
    /// </summary>
    /// <param name="searchFields">A collection of field names to include in the search scope.</param>
    /// <returns>The updated builder instance.</returns>
    public ISearchOptionsBuilder WithSearchFields(IList<string>? searchFields)
    {
        _searchFields = searchFields;
        return this;
    }

    /// <summary>
    /// Specifies which facets should be included in the search response.
    /// </summary>
    /// <param name="facets">A collection of facet field names.</param>
    /// <returns>The updated builder instance.</returns>
    public ISearchOptionsBuilder WithFacets(IList<string>? facets)
    {
        _facets = facets;
        return this;
    }

    /// <summary>
    /// Applies filter expressions to constrain search results.
    /// </summary>
    /// <param name="filters">
    /// Collection of domain-specific filter expressions, each including a name and corresponding set of values.
    /// These are transformed into Azure Search filter syntax during build.
    /// </param>
    /// <returns>The updated builder instance.</returns>
    public ISearchOptionsBuilder WithFilters(IList<FilterRequest>? filters)
    {
        _filters = filters;
        return this;
    }

    /// <summary>
    /// Assigns a pre-validated <see cref="SortOrder"/> instance to the builder.
    /// This allows external composition of sort logic prior to injection.
    /// </summary>
    /// <param name="sortOrder">
    /// A fully constructed <see cref="SortOrder"/> object representing the desired field and direction.
    /// </param>
    /// <returns>
    /// The updated builder instance for fluent chaining.
    /// </returns>
    public ISearchOptionsBuilder WithSortOrder(SortOrder sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    /// <summary>
    /// Builds and returns a configured instance of <see cref="SearchOptions"/> 
    /// ready for execution in an Azure Search query.
    /// </summary>
    /// <returns>
    /// A fully populated <see cref="SearchOptions"/> object reflecting the current 
    /// state of the builder's configuration.
    /// </returns>
    public SearchOptions Build()
    {
        _searchOptions.SearchMode = _searchMode;
        _searchOptions.Size = _size;
        _searchOptions.Skip = _offset;
        _searchOptions.IncludeTotalCount = _includeTotalCount;

        AddSortOrderIfAny();
        AddSearchFieldsIfAny();
        AddFacetsIfAny();
        ApplyFiltersIfAny();

        return _searchOptions;
    }

    /// <summary>
    /// Adds a sort expression to the <see cref="SearchOptions.OrderBy"/> collection
    /// if a valid <see cref="SortOrder"/> has been configured.
    /// </summary>
    private void AddSortOrderIfAny()
    {
        if (_sortOrder != null)
        {
            _searchOptions.OrderBy.Add(_sortOrder.ToString());
        }
    }

    /// <summary>
    /// Adds configured search field names to the <see cref="SearchOptions.SearchFields"/>
    /// collection. Only these fields will be queried by the Azure Search engine.
    /// </summary>
    private void AddSearchFieldsIfAny()
    {
        if (_searchFields is { Count: > 0 })
        {
            foreach (string field in _searchFields)
            {
                _searchOptions.SearchFields.Add(field);
            }
        }
    }

    /// <summary>
    /// Adds facet definitions to the <see cref="SearchOptions.Facets"/> collection if any.
    /// Facets allow aggregation of results by a specific field (e.g., category counts).
    /// </summary>
    private void AddFacetsIfAny()
    {
        if (_facets is { Count: > 0 })
        {
            foreach (string facet in _facets)
            {
                _searchOptions.Facets.Add(facet);
            }
        }
    }

    /// <summary>
    /// If one or more filters have been defined and a filter builder is available,
    /// constructs a filter expression string and assigns it to <see cref="SearchOptions.Filter"/>.
    /// This controls which documents are included/excluded before scoring.
    /// </summary>
    private void ApplyFiltersIfAny()
    {
        if (_filters?.Count > 0 && _searchFilterExpressionsBuilder != null)
        {
            IEnumerable<SearchFilterRequest> filterRequests =
                _filters.Select(filterRequest =>
                    new SearchFilterRequest(filterRequest.FilterName, filterRequest.FilterValues));

            _searchOptions.Filter =
                _searchFilterExpressionsBuilder
                    .BuildSearchFilterExpressions(filterRequests);
        }
    }
}
