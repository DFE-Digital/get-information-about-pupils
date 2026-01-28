using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides a stubbed instance of <see cref="SearchServiceAdapterRequest"/> for use in unit tests.
/// This test double simulates a typical search request payload, enabling deterministic test scenarios
/// without relying on real user input or external configuration.
/// </summary>
internal static class SearchServiceAdapterRequestTestDouble
{
    /// <summary>
    /// Returns a stubbed <see cref="SearchServiceAdapterRequest"/> with default values.
    /// Allows optional injection of filter criteria to test filtering logic in isolation.
    /// </summary>
    /// <param name="filters">Optional list of filter requests to include in the stub.</param>
    /// <returns>A preconfigured search request object for test use.</returns>
    public static SearchServiceAdapterRequest Stub(string searchIndexKey, IList<FilterRequest>? filters = null) =>
        new(
            searchIndexKey: searchIndexKey,                                     // Stubbed index-key for index-options lookup
            "searchKeyword",                                    // Simulated keyword for search input
            ["searchField1", "searchField2"],                   // Fields to search against
            new SortOrder("DOB", "asc", ["DOB", "Surname"]),    // Sort by DOB ascending, fallback to Surname
            ["facet1", "facet2"],                               // Facets to include in the response
            filters                                             // Optional filters injected for test specificity
        );
}
