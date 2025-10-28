using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.Options;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
using Microsoft.Extensions.Configuration;

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
            ["AzureSearchOptions:SearchIndex"] = "FE_INDEX_NAME",
            ["AzureSearchOptions:SearchMode"] = "0",                // Typically represents 'Any' or 'All'
            ["AzureSearchOptions:Size"] = "40000",                  // Max number of results
            ["AzureSearchOptions:IncludeTotalCount"] = "true",      // Whether to include result count
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
        CosmosDbOptions options = CosmosDbOptionsProvider.DefaultLocalOptions();

        Dictionary<string, string?> configurationOptions = new()
        {
            ["RepositoryOptions:ConnectionMode"] = "1",
            ["RepositoryOptions:EndpointUri"] = options.Uri.ToString(),
            ["RepositoryOptions:PrimaryKey"] = options.Key,
            ["RepositoryOptions:DatabaseId"] = options.DatabaseNames.Single()
        };

        // Add RepositoryOptions:Containers[]
        for (int index = 0; index < options.Containers.Count; index++)
        {
            string containerKey = options.Containers[index].ContainerName;
            string partitionKey = options.Containers[index].PartitionKey;

            configurationOptions.TryAdd($"RepositoryOptions:Containers:{index}:{containerKey}:ContainerName", containerKey);
            configurationOptions.TryAdd($"RepositoryOptions:Containers:{index}:{containerKey}:PartitionKey", partitionKey);
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
