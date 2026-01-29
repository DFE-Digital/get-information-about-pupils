using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Features.Search.Shared.Filters.FilterRegistration;

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
            string normalizedType = NormalizeFilterType(filter.FilterType.ToString());

            if (_handlers.TryGetValue(normalizedType, out IFilterHandler handler))
            {
                handler.Apply(filter, model, requestFilters);
            }
        }
    }

    /// <summary>
    /// Normalizes raw filter type strings to their canonical registry keys.
    /// This ensures consistent handler resolution regardless of input casing or aliasing.
    /// </summary>
    /// <param name="rawFilterType">The raw filter type string from the filter meta-data.</param>
    /// <returns>A normalized filter key used to look up the appropriate handler.</returns>
    private static string NormalizeFilterType(string rawFilterType) =>
        rawFilterType switch
        {
            // Map legacy or UI-facing filter names to internal registry keys
            "Surname" => FilterKeys.SurnameLC,
            "Forename" => FilterKeys.ForenameLC,
            "Dob" => FilterKeys.Dob,

            // Default: return the original string if no mapping is defined
            _ => rawFilterType
        };

    /// <summary>
    /// Centralized constants for semantic filter keys used in the filter handler registry.
    /// These keys act as canonical identifiers for mapping filters to their logic handlers.
    /// </summary>
    internal static class FilterKeys
    {
        /// <summary>
        /// Canonical key for date of birth filters.
        /// Used to resolve handlers that process DOB-related search input.
        /// </summary>
        public const string Dob = "DOB";

        /// <summary>
        /// Canonical key for surname filters with lowercase normalization.
        /// Ensures consistent matching regardless of input casing.
        /// </summary>
        public const string SurnameLC = "SurnameLC";

        /// <summary>
        /// Canonical key for forename filters with lowercase normalization.
        /// Used to apply case-insensitive logic to forename-based filters.
        /// </summary>
        public const string ForenameLC = "ForenameLC";
    }
}
