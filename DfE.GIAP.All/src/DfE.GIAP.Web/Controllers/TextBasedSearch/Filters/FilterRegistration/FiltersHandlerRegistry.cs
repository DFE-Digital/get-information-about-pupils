using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.Handlers;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.FilterRegistration;

/// <summary>
/// Registry that maps semantic filter keys (e.g., "DOB", "Surname") to their corresponding handler implementations.
/// Delegates filter application logic to the appropriate handler based on filter type.
/// </summary>
public class FilterHandlerRegistry : IFilterHandlerRegistry
{
    /// <summary>
    /// Internal dictionary of registered filter handlers keyed by semantic filter name.
    /// </summary>
    private readonly Dictionary<string, IFilterHandler> _handlers;

    /// <summary>
    /// Constructs a registry with a dictionary of filter handlers.
    /// </summary>
    /// <param name="handlers">Dictionary mapping semantic filter keys to handler instances.</param>
    /// <exception cref="ArgumentNullException">Thrown if the handlers dictionary is null.</exception>
    public FilterHandlerRegistry(Dictionary<string, IFilterHandler> handlers)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
    }

    /// <summary>
    /// Applies all registered filters to the requestFilters dictionary using their corresponding handlers.
    /// </summary>
    /// <param name="filters">List of active filters to apply.</param>
    /// <param name="model">View model containing user-entered search inputs.</param>
    /// <param name="requestFilters">Dictionary to populate with structured filter values.</param>
    public void ApplyFilters(
        List<CurrentFilterDetail> filters,
        LearnerTextSearchViewModel model,
        Dictionary<string, string[]> requestFilters)
    {
        foreach (CurrentFilterDetail filter in filters)
        {
            #region BODGE - this can be removed once we sort the other filters
            // NOTE: I'm applying a bodge here to simplify the call to the correct facet. Once we sort the other
            // filters out, we can remove this!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            string filterType = filter.FilterType.ToString();

            if (filterType == "Surname" || filterType == "Forename")
            {
                filterType = $"{filterType}LC";
            }

            if (filterType == "Dob")
            {
                filterType = "DOB";
            }
            #endregion

            // Resolve handler based on the filter type's string representation.
            if (_handlers.TryGetValue(filterType, out IFilterHandler handler))
            {
                // Delegate filter logic to the matched handler.
                handler.Apply(filter, model, requestFilters);
            }
        }
    }
}
