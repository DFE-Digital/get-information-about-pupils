using DfE.GIAP.Web.Features.Search.Options.Search;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides a stubbed instance of <see cref="AzureSearchOptions"/> for use in unit tests.
/// This test double isolates configuration dependencies, enabling deterministic test behavior
/// without relying on external config files or runtime bindings.
/// </summary>
internal static class SearchOptionsTestDouble
{
    /// <summary>
    /// Returns a stubbed <see cref="AzureSearchOptions"/> object with default values
    /// tailored for testing the "Further Education" search index.
    /// </summary>
    public static SearchOptions Stub()
    {
        Dictionary<string, SearchIndexOptions> indexes = new()
        {
            {
                "further-education",
                new SearchIndexOptions()
                {
                    SearchCriteria = new SearchCriteriaOptions()
                    {
                        SearchMode = 0,// Default search mode (e.g., 'Any' or 'All' depending on enum)
                        Size = 100, // Max number of results to return
                        IncludeTotalCount = true, // Enables total count metadata in search response
                        SearchIndex = "idx-further-education" // Target index for test scenarios
                    }
                }
            }
        };

        return new SearchOptions()
        {
            Indexes = indexes
        };
    }
}
