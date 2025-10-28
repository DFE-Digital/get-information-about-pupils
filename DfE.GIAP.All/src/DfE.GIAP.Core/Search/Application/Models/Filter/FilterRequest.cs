﻿namespace DfE.GIAP.Core.Search.Application.Models.Filter;

/// <summary>
/// Represents a search filter and its associated values, used to refine query results.
/// </summary>
public class FilterRequest
{
    private readonly IList<object> _filterValues;

    /// <summary>
    /// The name or key of the filter (e.g. "Name", "Sex").
    /// </summary>
    public string FilterName { get; }

    /// <summary>
    /// The values to match against the filter. Read-only wrapper to prevent mutation.
    /// </summary>
    public IList<object> FilterValues => _filterValues.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterRequest"/> class.
    /// </summary>
    /// <param name="filterName">Name of the filter.</param>
    /// <param name="filterValues">List of filter values.</param>
    /// <exception cref="ArgumentNullException">Thrown if filterName or filterValues is null.</exception>
    public FilterRequest(string filterName, IList<object> filterValues)
    {
        FilterName = filterName ??
            throw new ArgumentNullException(nameof(filterName));
        _filterValues = filterValues ??
            throw new ArgumentNullException(nameof(filterValues));
    }
}
