using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters.FilterRegistration;

/// <summary>
/// Contract for a registry that delegates filter application to registered handlers.
/// Enables dynamic resolution of filter logic based on filter type.
/// </summary>
public interface IFilterHandlerRegistry
{
    /// <summary>
    /// Applies a set of filters to the search model and populates the requestFilters dictionary.
    /// Each filter is delegated to its corresponding handler based on filter type.
    /// </summary>
    /// <param name="filters">List of filters currently active in the search context.</param>
    /// <param name="model">View model containing user-entered search criteria.</param>
    /// <param name="requestFilters">Dictionary to populate with structured filter values for downstream processing.</param>
    void ApplyFilters(
        List<CurrentFilterDetail> filters,
        LearnerTextSearchViewModel model,
        Dictionary<string, string[]> requestFilters);
}
