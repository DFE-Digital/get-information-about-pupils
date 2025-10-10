namespace DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;

/// <summary>
/// Provides stubbed configuration values for search index-related options
/// used in integration tests.
/// </summary>
public static class SearchIndexOptionsStub
{
    /// <summary>
    /// Returns a dictionary of configuration key-value pairs for search index setup.
    /// </summary>
    /// <param name="searchIndexUrl">The base URL of the search index service.</param>
    /// <returns>A dictionary representing configuration settings.</returns>
    public static Dictionary<string, string> StubFor(string searchIndexUrl) =>
        new()
        {
            // SearchIndexOptions: Basic connection and index names
            ["SearchIndexOptions:Url"] = searchIndexUrl,
            ["SearchIndexOptions:Key"] = "SEFSOFOIWSJFSO",
            ["SearchIndexOptions:Indexes:npd:Name"] = "npd",
            ["SearchIndexOptions:Indexes:pupil-premium:Name"] = "pupil-premium-index",
            ["SearchIndexOptions:Indexes:further-education:Name"] = "further-education",
            //
            // SearchCriteria: Fields and facets used in search queries
            ["SearchCriteria:SearchFields:0"] = "Forename",
            ["SearchCriteria:SearchFields:1"] = "Surname",
            ["SearchCriteria:Facets:0"] = "ForenameLC",
            ["SearchCriteria:Facets:1"] = "SurnameLC",
            ["SearchCriteria:Facets:2"] = "Gender",
            ["SearchCriteria:Facets:3"] = "Sex",
            //
            // AzureSearchOptions: Parameters controlling search behavior
            ["AzureSearchOptions:SearchIndex"] = "further-education",
            ["AzureSearchOptions:SearchMode"] = "0",                // Typically represents 'Any' or 'All'
            ["AzureSearchOptions:Size"] = "40000",                  // Max number of results
            ["AzureSearchOptions:IncludeTotalCount"] = "true",      // Whether to include result count
            //
            // AzureSearchConnectionOptions: Connection details for Azure Search
            ["AzureSearchConnectionOptions:EndpointUri"] = searchIndexUrl,
            ["AzureSearchConnectionOptions:Credentials"] = "SEFSOFOIWSJFSO"
        };
}
