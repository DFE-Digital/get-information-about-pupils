using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.Features.Search.Shared.Filters.FilterRegistration;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters;

/// <summary>
/// Creates a structured filter request payload for learner search.
/// Delegates filter logic to registered handlers via the IFilterHandlerRegistry.
/// </summary>
internal sealed class FiltersRequestFactory : IFiltersRequestFactory
{
    /// <summary>
    /// Registry that maps filter types to their corresponding handler implementations.
    /// </summary>
    private readonly IFilterHandlerRegistry _registry;

    /// <summary>
    /// Constructs a FiltersRequestBuilder with a filter handler registry.
    /// </summary>
    /// <param name="registry">Registry used to apply semantic filter logic.</param>
    public FiltersRequestFactory(IFilterHandlerRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Generates a dictionary of structured filter values for API requests.
    /// Applies all current filters using the registry and populates the requestFilters dictionary.
    /// </summary>
    /// <param name="model">View model containing user-entered search inputs.</param>
    /// <param name="currentFilters">List of active filters to apply.</param>
    /// <returns>Dictionary of filter keys and values for downstream search APIs.</returns>
    public Dictionary<string, string[]> GenerateFilterRequest(
        LearnerTextSearchViewModel model,
        List<CurrentFilterDetail> currentFilters)
    {
        // Initialize an empty filter dictionary.
        Dictionary<string, string[]> requestFilters = [];

        // Apply filters only if the list is non-null and non-empty.
        if (currentFilters != null && currentFilters.Count != 0)
        {
            _registry.ApplyFilters(currentFilters, model, requestFilters);
        }

        return requestFilters;
    }
}
