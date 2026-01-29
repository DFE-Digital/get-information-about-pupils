using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Sort;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;

/// <summary>
/// Provides a stubbed <see cref="SortOrder"/> instance for use in unit tests.
/// Simulates a typical sort configuration to validate sorting logic and adapter behavior.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class SortOrderTestDouble
{
    /// <summary>
    /// Returns a stubbed <see cref="SortOrder"/> object with predefined values:
    /// - Sorts by "Forename" in descending order
    /// - Declares "Forename", "Surname", and "DOB" as valid sort fields
    /// Useful for testing sort validation, query shaping, and symbolic traceability.
    /// </summary>
    public static SortOrder Stub() =>
        new(
            sortField: "Forename",                          // Primary sort field
            sortDirection: "desc",                          // Sort direction
            validSortFields: ["Forename", "Surname", "DOB"] // Allowed fields for sorting
        );
}
