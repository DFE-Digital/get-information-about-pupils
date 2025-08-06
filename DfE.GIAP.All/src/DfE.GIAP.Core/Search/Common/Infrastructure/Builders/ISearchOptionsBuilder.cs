using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Common.Application.Adapters.Model;

namespace DfE.GIAP.Core.Search.Common.Infrastructure.Builders;

/// <summary>
/// Defines an abstraction for constructing tailored <see cref="SearchOptions" /> instances.
/// </summary>
public interface ISearchOptionsBuilder
{
    /// <summary>
    /// Specifies the number of results to retrieve.
    /// </summary>
    /// <param name="size">The maximum number of search results to return.</param>
    /// <returns>An updated builder instance.</returns>
    ISearchOptionsBuilder WithSize(int? size);

    /// <summary>
    /// Specifies how many results to skip from the beginning of the result set.
    /// Defaults to zero (no skipped results).
    /// </summary>
    /// <param name="offset">Number of records to skip.</param>
    /// <returns>An updated builder instance.</returns>
    ISearchOptionsBuilder WithOffset(int offset = 0);

    /// <summary>
    /// Sets the search mode: whether all terms must match or any can match.
    /// </summary>
    /// <param name="searchMode">Search mode to apply: <see cref="SearchMode.Any"/> or <see cref="SearchMode.All"/>.</param>
    /// <returns>An updated builder instance.</returns>
    ISearchOptionsBuilder WithSearchMode(SearchMode searchMode);

    /// <summary>
    /// Configures whether to include the total result count in the search response.
    /// </summary>
    /// <param name="includeTotalCount">True to include total count, false to exclude.</param>
    /// <returns>An updated builder instance.</returns>
    ISearchOptionsBuilder WithIncludeTotalCount(bool? includeTotalCount);

    /// <summary>
    /// Specifies which fields to target in the search operation.
    /// </summary>
    /// <param name="searchFields">A list of field names to apply the search against.</param>
    /// <returns>An updated builder instance.</returns>
    ISearchOptionsBuilder WithSearchFields(IList<string>? searchFields);

    /// <summary>
    /// Sets which facets to include in the search results for aggregation.
    /// </summary>
    /// <param name="facets">A list of facet expressions.</param>
    /// <returns>An updated builder instance.</returns>
    ISearchOptionsBuilder WithFacets(IList<string>? facets);

    /// <summary>
    /// Applies one or more filters to constrain the search results.
    /// </summary>
    /// <param name="filters">A list of filters to apply.</param>
    /// <returns>An updated builder instance.</returns>
    ISearchOptionsBuilder WithFilters(IList<FilterRequest>? filters);

    /// <summary>
    /// Builds the configured <see cref="SearchOptions"/> instance.
    /// </summary>
    /// <returns>A fully configured <see cref="SearchOptions"/> object.</returns>
    SearchOptions Build();
}

