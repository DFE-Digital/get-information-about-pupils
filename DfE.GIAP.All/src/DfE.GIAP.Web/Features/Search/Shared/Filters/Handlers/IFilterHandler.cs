using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;

/// <summary>
/// Defines a contract for applying semantic filter logic to learner search.
/// Each implementation handles a specific filter type (e.g., DOB, Gender, Surname).
/// </summary>
public interface IFilterHandler
{
    /// <summary>
    /// Applies filter-specific logic to populate the requestFilters dictionary.
    /// May also update the model to reflect parsed or normalized filter values.
    /// </summary>
    /// <param name="filter">Meta-data describing the current filter (e.g., name, type).</param>
    /// <param name="model">View model containing user-entered search inputs.</param>
    /// <param name="requestFilters">Dictionary to populate with structured filter values for API requests.</param>
    void Apply(
        CurrentFilterDetail filter,
        LearnerTextSearchViewModel model,
        Dictionary<string, string[]> requestFilters);
}
