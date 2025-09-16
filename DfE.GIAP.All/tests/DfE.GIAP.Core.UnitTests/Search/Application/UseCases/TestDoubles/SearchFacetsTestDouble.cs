using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;

/// <summary>
/// Provides a stubbed instance of <see cref="SearchFacets"/> for use in unit tests.
/// Simulates a typical facet result payload with predefined values,
/// enabling deterministic testing of facet mapping and result shaping logic.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SearchFacetsTestDouble
{
    /// <summary>
    /// Constructs a <see cref="SearchFacets"/> object containing a single facet named "name"
    /// with one result value "value1" and a count of 1.
    /// Useful for validating facet rendering, filtering, and adapter behavior.
    /// </summary>
    public static SearchFacets Stub()
    {
        List<SearchFacet> facets =
        [
            new SearchFacet("name", [new FacetResult("value1", 1)])
        ];

        return new SearchFacets(facets);
    }
}
