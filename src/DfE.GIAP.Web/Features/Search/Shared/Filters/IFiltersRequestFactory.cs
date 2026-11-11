using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters;

/// <summary>
/// Defines a contract for creating structured filter request payloads.
/// Implementations convert user-entered search inputs and active filters into API-ready dictionaries.
/// </summary>
public interface IFiltersRequestFactory
{
    /// <summary>
    /// Generates a dictionary of filter keys and values based on the current search model and active filters.
    /// Used to construct query parameters for downstream learner search APIs.
    /// </summary>
    /// <param name="model">View model containing user-entered search inputs.</param>
    /// <param name="currentFilters">List of active filters to apply.</param>
    /// <returns>Dictionary of structured filter values keyed by semantic meaning (e.g., DOB, Gender).</returns>
    Dictionary<string, string[]> GenerateFilterRequest(
        LearnerTextSearchViewModel model,
        List<CurrentFilterDetail> currentFilters);
}
