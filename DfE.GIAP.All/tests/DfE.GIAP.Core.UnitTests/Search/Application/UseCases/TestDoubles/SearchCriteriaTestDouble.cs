using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;

/// <summary>
/// Provides a stubbed instance of <see cref="SearchCriteria"/> for use in unit tests.
/// Simulates a typical search payload with predefined facets and search fields,
/// enabling deterministic test scenarios without relying on real user input.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SearchCriteriaTestDouble
{
    /// <summary>
    /// Returns a stubbed <see cref="SearchCriteria"/> object with hardcoded values.
    /// Useful for testing search adapter logic, filter construction, and facet mapping.
    /// </summary>
    public static SearchCriteria Stub() => new()
    {
        Facets = ["FIELD1", "FIELD2", "FIELD3"],       // Simulated facet fields
        SearchFields = ["FACET1", "FACET2", "FACET3"]  // Simulated searchable fields
    };
}
