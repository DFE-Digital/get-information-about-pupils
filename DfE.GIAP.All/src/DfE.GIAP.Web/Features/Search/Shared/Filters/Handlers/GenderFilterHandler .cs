using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.ViewModels.Search;
using DfE.GIAP.Domain.Search.Learner;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;

/// <summary>
/// Handles gender-based filtering logic for learner search.
/// Appends the initial character of the filter name to the model and request filters under a target key.
/// </summary>
public class GenderFilterHandler : IFilterHandler
{
    /// <summary>
    /// The key used to store gender filter values in both model.Filters and requestFilters.
    /// </summary>
    private readonly string _targetKey;

    /// <summary>
    /// Constructs a gender filter handler with a target filter key.
    /// </summary>
    /// <param name="targetKey">Key to use in the filter list (e.g., "Gender", "Sex").</param>
    public GenderFilterHandler(string targetKey)
    {
        _targetKey = targetKey;
    }

    /// <summary>
    /// Applies the gender filter by extracting the initial character from the filter name
    /// and updating both the model and the requestFilters dictionary.
    /// </summary>
    /// <param name="filter">Current filter meta-data (e.g., "Male", "Female").</param>
    /// <param name="model">View model containing user-entered search inputs.</param>
    /// <param name="requestFilters">Dictionary to populate with gender filter values for downstream search.</param>
    public void Apply(
        CurrentFilterDetail filter,
        LearnerTextSearchViewModel model,
        Dictionary<string, string[]> requestFilters)
    {
        // Extract the first character of the filter name (e.g., "M" or "F").
        string initial = filter.FilterName.Substring(0, 1);

        // Ensure model.Filters is initialized.
        model.Filters ??= [];

        // Find or create the FilterData entry for the target key.
        FilterData filterData = model.Filters.FirstOrDefault(filter => filter.Name == _targetKey);

        if (filterData == null)
        {
            filterData = new FilterData { Name = _targetKey };
            model.Filters.Add(filterData);
        }

        // Add the initial if it's not already present.
        if (!filterData.Items.Any(filterDataItem => filterDataItem.Value == initial))
        {
            filterData.Items.Add(new FilterDataItem { Value = initial });
        }

        // Sync with requestFilters.
        requestFilters[_targetKey] = filterData.Items.Select(item => item.Value).ToArray();
    }
}
