namespace DfE.GIAP.SharedTests.TestDoubles.Configuration;

public static class ConfigurationTestDoubles
{
    public static IConfigurationBuilder DefaultConfigurationBuilder() => new ConfigurationBuilder();

    public static IConfigurationBuilder WithFeatureFlagsOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> featureFlagStubConfig = new()
        {
            ["FeatureFlagAppConfigUrl"] = "Endpoint=https://featureflags.azconfig.io;Id=ID;Secret=SECRET",
        };

        builder.AddInMemoryCollection(featureFlagStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithStorageAccountOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> storageAccountStubConfig = new()
        {
            ["StorageAccountName"] = "AZURE_STORAGE_ACCOUNTNAME",
            ["StorageAccountKey"] = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;",
            ["StorageContainerName"] = "AZURE_STORAGE_CONTAINERNAME",
        };

        builder.AddInMemoryCollection(storageAccountStubConfig);

        return builder;
    }

    // TODO migrate to using AzureSearchIndexOptions
    public static IConfigurationBuilder WithSearchIndexOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> searchIndexStubConfig = new()
        {
            ["SearchIndexOptions:Url"] = "https://localhost:8443",
            ["SearchIndexOptions:Key"] = "ANY_KEY",
            ["SearchIndexOptions:Indexes:npd:Name"] = "NPD_INDEX_NAME",
            ["SearchIndexOptions:Indexes:pupil-premium:Name"] = "PUPIL_PREMIUM_INDEX_NAME",
            ["SearchIndexOptions:Indexes:further-education:Name"] = "FE_INDEX_NAME",
        };

        builder.AddInMemoryCollection(searchIndexStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithAzureSearchConnectionOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> searchConnectionStubConfig = new()
        {
            // AzureSearchConnectionOptions: Connection details for Azure Search
            ["AzureSearchConnectionOptions:EndpointUri"] = "https://localhost:8443",
            ["AzureSearchConnectionOptions:Credentials"] = "SEFSOFOIWSJFSO"
        };

        builder.AddInMemoryCollection(searchConnectionStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithAzureSearchOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> azureSearchConnectionStubConfig = new()
        {
            // AzureSearchOptions: Parameters controlling search behavior
            ["AzureSearchOptions:Indexes:further-education:SearchIndex"] = "FE_INDEX_NAME",
            ["AzureSearchOptions:Indexes:further-education:SearchMode"] = "0",                // Typically represents 'Any' or 'All'
            ["AzureSearchOptions:Indexes:further-education:Size"] = "40000",                  // Max number of results
            ["AzureSearchOptions:Indexes:further-education:IncludeTotalCount"] = "true",      // Whether to include result count
        };

        builder.AddInMemoryCollection(azureSearchConnectionStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithSearchCriteriaOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> searchCriteriaStub = new()
        {
            // SearchCriteria: Fields and facets used in search queries
            ["SearchCriteria:SearchFields:0"] = "Forename",
            ["SearchCriteria:SearchFields:1"] = "Surname",
            ["SearchCriteria:Facets:0"] = "ForenameLC",
            ["SearchCriteria:Facets:1"] = "SurnameLC",
            ["SearchCriteria:Facets:2"] = "Gender",
            ["SearchCriteria:Facets:3"] = "Sex",
        };

        builder.AddInMemoryCollection(searchCriteriaStub);

        return builder;
    }

    public static IConfigurationBuilder WithLocalCosmosDbOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> configurationOptions = new()
        {
            ["RepositoryOptions:ConnectionMode"] = "1",
            ["RepositoryOptions:EndpointUri"] = "https://localhost:8081",
            ["RepositoryOptions:PrimaryKey"] = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            ["RepositoryOptions:DatabaseId"] = "giapsearch"
        };

        Dictionary<string, string> containerOptions = new()
        {
            { "application-data", "/DOCTYPE" },
            { "news", "/id" },
            { "users", "/id" },
            { "mypupils", "/id" }
            /*new CosmosDbContainerOptions("further-education", "/ULN"),
                new CosmosDbContainerOptions("pupil-noskill", "/PupilMatchingRef"),
                new CosmosDbContainerOptions("pupil-premium-v2", "/PupilMatchingRef"),
                new CosmosDbContainerOptions("reference", "/DOCTYPE")*/
        };

        foreach (KeyValuePair<string, string> item in containerOptions)
        {
            configurationOptions.TryAdd($"RepositoryOptions:Containers:{item.Key}:ContainerName", item.Key);
            configurationOptions.TryAdd($"RepositoryOptions:Containers:{item.Key}:PartitionKey", containerOptions[item.Key]);
        }
        builder.AddInMemoryCollection(configurationOptions);
        return builder;
    }

    private sealed class ContainerOptions(string ContainerName, string PartitionKey);

    public static IConfigurationBuilder WithDsiOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> dsiStubConfig = new()
        {
            ["DsiClientId"] = "test-client-id",
            ["DsiClientSecret"] = "client_secret",
            ["DsiMetadataAddress"] = "https://integrationtest.example",
            ["DsiRedirectUrlAfterSignout"] = "REDIRECT_URL",
            ["DsiServiceId"] = "SERVICE_ID",
        };

        builder.AddInMemoryCollection(dsiStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithFilterKeyToFilterExpressionMapOptions(this IConfigurationBuilder builder)
    {

        Dictionary<string, string?> configDict = new()
        {
            ["FilterKeyToFilterExpressionMapOptions:FilterChainingLogicalOperator"] = "AndLogicalOperator",

            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:SurnameLC:FilterExpressionKey"] = "SearchCollectionValuedFilterExpression",
            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:SurnameLC:FilterExpressionValuesDelimiter"] = ",",

            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:ForenameLC:FilterExpressionKey"] = "SearchCollectionValuedFilterExpression",
            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:ForenameLC:FilterExpressionValuesDelimiter"] = ",",

            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:DOB:FilterExpressionKey"] = "SearchByEqualityFilterExpression",
            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:DOB:FilterExpressionValuesDelimiter"] = ",",

            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:DOBYearMonth:FilterExpressionKey"] = "SearchByEqualityFilterExpression",
            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:DOBYearMonth:FilterExpressionValuesDelimiter"] = ",",

            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:DOBYear:FilterExpressionKey"] = "SearchByEqualityFilterExpression",
            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:DOBYear:FilterExpressionValuesDelimiter"] = ",",

            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:Gender:FilterExpressionKey"] = "SearchByEqualityFilterExpression",
            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:Gender:FilterExpressionValuesDelimiter"] = ",",

            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:Sex:FilterExpressionKey"] = "SearchByEqualityFilterExpression",
            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:Sex:FilterExpressionValuesDelimiter"] = ",",

            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:ULN:FilterExpressionKey"] = "SearchInFilterExpression",
            ["FilterKeyToFilterExpressionMapOptions:SearchFilterToExpressionMap:ULN:FilterExpressionValuesDelimiter"] = ",",
        };


        builder.AddInMemoryCollection(configDict);
        return builder;
    }
}
