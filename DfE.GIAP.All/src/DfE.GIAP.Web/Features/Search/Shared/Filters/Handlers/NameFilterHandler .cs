using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;

/// <summary>
/// Handles name-based filtering logic for learner search.
/// Appends distinct name values to the request filter under a specified key.
/// </summary>
internal sealed class NameFilterHandler : IFilterHandler
{
    /// <summary>
    /// The key used to store name filter values in the requestFilters dictionary.
    /// </summary>
    private readonly string _targetKey;

    /// <summary>
    /// Constructs a name filter handler with a target filter key.
    /// </summary>
    /// <param name="targetKey">Key to use in the requestFilters dictionary (e.g., "surname").</param>
    public NameFilterHandler(string targetKey)
    {
        _targetKey = targetKey;
    }

    /// <summary>
    /// Applies the name filter by appending the filter value to the requestFilters dictionary.
    /// Ensures values are distinct and non-empty.
    /// </summary>
    /// <param name="filter">Meta-data describing the current filter (e.g., name, type).</param>
    /// <param name="model">View model containing user-entered search inputs.</param>
    /// <param name="requestFilters">Dictionary to populate with name filter values.</param>
    public void Apply(
        CurrentFilterDetail filter,
        LearnerTextSearchViewModel model,
        Dictionary<string, string[]> requestFilters)
    {
        // Skip if the filter name is null, empty, or whitespace.
        if (string.IsNullOrWhiteSpace(filter.FilterName))
        {
            return;
        }

        // Retrieve existing values for the target key, or initialize an empty array.
        if (!requestFilters.TryGetValue(_targetKey, out string[] existing))
        {
            existing = [];
        }

        // Append the new name value, ensuring no duplicates.
        requestFilters[_targetKey] =
            existing.Append(filter.FilterName).Distinct().ToArray();
    }
}
