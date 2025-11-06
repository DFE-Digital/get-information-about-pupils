using DfE.GIAP.Core.Search.Infrastructure.Options;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides a stubbed instance of <see cref="AzureSearchOptions"/> for use in unit tests.
/// This test double isolates configuration dependencies, enabling deterministic test behavior
/// without relying on external config files or runtime bindings.
/// </summary>
internal static class AzureSearchOptionsTestDouble
{
    /// <summary>
    /// Returns a stubbed <see cref="AzureSearchOptions"/> object with default values
    /// tailored for testing the "Further Education" search index.
    /// </summary>
    public static AzureSearchOptions Stub()
    {
        Dictionary<string, AzureSearchIndexOptions> indexes = new()
        {
            {
                "further-education",
                new AzureSearchIndexOptions()
                {
                    SearchMode = 0,                         // Default search mode (e.g., 'Any' or 'All' depending on enum)
                    Size = 100,                             // Max number of results to return
                    IncludeTotalCount = true,               // Enables total count metadata in search response
                    SearchIndex = "idx-further-education"   // Target index for test scenarios
                }
            }
        };

        return new AzureSearchOptions()
        {
            Indexes = indexes
        };
    }
}
