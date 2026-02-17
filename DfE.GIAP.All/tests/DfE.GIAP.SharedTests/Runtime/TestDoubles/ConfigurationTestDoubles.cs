namespace DfE.GIAP.SharedTests.Runtime.TestDoubles;

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

    public static IConfigurationBuilder WithSearchOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> azureSearchConnectionStubConfig = new()
        {
            // AzureSearchOptions: Parameters controlling search behavior
            ["SearchOptions:Indexes:further-education-text:SearchCriteria:SearchIndex"] = "FE_INDEX_NAME",
            ["SearchOptions:Indexes:further-education-text:SearchCriteria:SearchMode"] = "0",
            ["SearchOptions:Indexes:further-education-text:SearchCriteria:Size"] = "40000",
            ["SearchOptions:Indexes:further-education-text:SearchCriteria:IncludeTotalCount"] = "true",
            ["SearchOptions:Indexes:npd-upn:SearchCriteria:SearchIndex"] = "NPD_INDEX_NAME",
            ["SearchOptions:Indexes:npd-upn:SearchCriteria:SearchMode"] = "0",
            ["SearchOptions:Indexes:npd-upn:SearchCriteria:Size"] = "40000",
            ["SearchOptions:Indexes:npd-upn:SearchCriteria:IncludeTotalCount"] = "true",
            ["SearchOptions:Indexes:npd-upn:SearchCriteria:SearchFields:0"] = "field1",
            ["SearchOptions:Indexes:pupil-premium-upn:SearchCriteria:SearchIndex"] = "PUPIL_PREMIUM_INDEX_NAME",
            ["SearchOptions:Indexes:pupil-premium-upn:SearchCriteria:SearchMode"] = "0",
            ["SearchOptions:Indexes:pupil-premium-upn:SearchCriteria:Size"] = "40000",
            ["SearchOptions:Indexes:pupil-premium-upn:SearchCriteria:IncludeTotalCount"] = "true",
            ["SearchOptions:Indexes:pupil-premium-upn:SearchCriteria:SearchFields:0"] = "field2",
        };

        builder.AddInMemoryCollection(azureSearchConnectionStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithSearchIndexNameOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string?> azureSearchConnectionStubConfig = new()
        {
            ["SearchIndexNamesOptions:Names:0"] = "FE_INDEX_NAME",
            ["SearchIndexNamesOptions:Names:1"] = "NPD_INDEX_NAME",
            ["SearchIndexNamesOptions:Names:2"] = "PUPIL_PREMIUM_INDEX_NAME",
        };

        builder.AddInMemoryCollection(azureSearchConnectionStubConfig);

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
