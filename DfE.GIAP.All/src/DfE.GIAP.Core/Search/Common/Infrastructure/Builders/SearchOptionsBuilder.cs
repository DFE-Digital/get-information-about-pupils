using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using DfE.GIAP.Core.Search.Common.Application.Adapters.Model;

namespace DfE.GIAP.Core.Search.Common.Infrastructure.Builders;

/// <summary>
/// Provides a concrete implementation of the <see cref="ISearchOptionsBuilder"/> abstraction,
/// which establishes a configured <see cref="SearchOptions" /> instance with prescribed behaviour.
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

    /// <summary>
    /// Injects a filter expression builder to construct domain-specific filter syntax.
    /// </summary>
    /// <param name="searchFilterExpressionsBuilder">
    /// Dependency for building Azure Search-compatible filter expressions from filter requests.
    /// </param>
    public SearchOptionsBuilder(ISearchFilterExpressionsBuilder? searchFilterExpressionsBuilder = null)
    {
        _searchFilterExpressionsBuilder = searchFilterExpressionsBuilder;
        _searchOptions = new SearchOptions();
    }

    /// <summary>
    /// Specifies the maximum number of search results to retrieve.
    /// </summary>
    /// <param name="size">
    /// Total number of records to return.
    /// </param>
    /// <returns>
    /// The updated builder instance.
    /// </returns>
    public ISearchOptionsBuilder WithSize(int? size)
    {
        _size = size;
        return this;
    }

    /// <summary>
    /// Defines the number of records to skip in the search response.
    /// </summary>
    /// <param name="offset">
    /// The initial number of results to skip. Defaults to zero.
    /// </param>
    /// <returns>
    /// The updated builder instance.
    /// </returns>
    public ISearchOptionsBuilder WithOffset(int offset = 0)
    {
        _offset = offset;
        return this;
    }

    /// <summary>
    /// Sets the search mode to determine how terms should match.
    /// </summary>
    /// <param name="searchMode">
    /// Specifies whether all or any search terms must match.
    /// </param>
    /// <returns>
    /// The updated builder instance.
    /// </returns>
    public ISearchOptionsBuilder WithSearchMode(SearchMode searchMode)
    {
        _searchMode = searchMode;
        return this;
    }

    /// <summary>
    /// Configures the response to include the total count of results.
    /// </summary>
    /// <param name="includeTotalCount">
    /// Boolean flag indicating whether total count should be included.
    /// </param>
    /// <returns>
    /// The updated builder instance.
    /// </returns>
    public ISearchOptionsBuilder WithIncludeTotalCount(bool? includeTotalCount)
    {
        _includeTotalCount = includeTotalCount;
        return this;
    }

    /// <summary>
    /// Specifies which fields should be searchable.
    /// </summary>
    /// <param name="searchFields">
    /// A collection of field names to include in the search scope.
    /// </param>
    /// <returns>
    /// The updated builder instance.
    /// </returns>
    public ISearchOptionsBuilder WithSearchFields(IList<string>? searchFields)
    {
        _searchFields = searchFields;
        return this;
    }

    /// <summary>
    /// Specifies which facets should be included in the search response.
    /// </summary>
    /// <param name="facets">
    /// A collection of facet field names.
    /// </param>
    /// <returns>
    /// The updated builder instance.
    /// </returns>
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
    /// <returns>
    /// The updated builder instance.
    /// </returns>
    public ISearchOptionsBuilder WithFilters(IList<FilterRequest>? filters)
    {
        _filters = filters;
        return this;
    }

    /// <summary>
    /// Builds and returns a configured instance of <see cref="SearchOptions"/>.
    /// </summary>
    /// <returns>
    /// A fully populated <see cref="SearchOptions"/> instance ready for Azure Search queries.
    /// </returns>
    public SearchOptions Build()
    {
        _searchOptions.SearchMode = _searchMode;
        _searchOptions.Size = _size;
        _searchOptions.Skip = _offset;
        _searchOptions.IncludeTotalCount = _includeTotalCount;

        _searchFields?.ToList().ForEach(_searchOptions.SearchFields.Add);
        _facets?.ToList().ForEach(_searchOptions.Facets.Add);

        if (_filters?.Count > 0 && _searchFilterExpressionsBuilder != null)
        {
            _searchOptions.Filter =
                _searchFilterExpressionsBuilder.BuildSearchFilterExpressions(
                    _filters.Select(filterRequest =>
                        new SearchFilterRequest(filterRequest.FilterName, filterRequest.FilterValues)));
        }

        return _searchOptions;
    }
}
