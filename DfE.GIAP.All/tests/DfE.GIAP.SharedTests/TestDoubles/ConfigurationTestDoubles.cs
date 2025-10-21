using Azure.Search.Documents.Indexes.Models;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace DfE.GIAP.SharedTests.TestDoubles;

public static class ConfigurationTestDoubles
{
    public static IConfigurationBuilder DefaultConfigurationBuilder() => new ConfigurationBuilder();

    private static void WithConfiguration(this IConfigurationBuilder builder, Dictionary<string, string> config)
    {
        builder.Add(
            new MemoryConfigurationSource() { InitialData = config! });
    }

    public static IConfigurationBuilder WithFeatureFlagsOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string> featureFlagStubConfig = new()
        {
            ["FeatureFlagAppConfigUrl"] = "Endpoint=https://featureflags.azconfig.io;Id=ID;Secret=SECRET",
        };

        builder.WithConfiguration(featureFlagStubConfig);

        return builder;
    }

    // NOTE these are not secrets just stubbed data
    public static IConfigurationBuilder WithStorageAccountOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string> storageAccountStubConfig = new()
        {
            ["StorageAccountName"] = "AZURE_STORAGE_ACCOUNTNAME",
            ["StorageAccountKey"] =
                "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;",
            ["StorageContainerName"] = "AZURE_STORAGE_CONTAINERNAME",
        };

        builder.WithConfiguration(storageAccountStubConfig);

        return builder;
    }

    // TODO migrate to using AzureSearchIndexOptions
    public static IConfigurationBuilder WithSearchIndexOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string> searchIndexStubConfig = new()
        {
            ["SearchIndexOptions:Url"] = "https://localhost:8443",
            ["SearchIndexOptions:Key"] = "ANY_KEY",
            ["SearchIndexOptions:Indexes:npd:Name"] = "NPD_INDEX_NAME",
            ["SearchIndexOptions:Indexes:pupil-premium:Name"] = "PUPIL_PREMIUM_INDEX_NAME",
            ["SearchIndexOptions:Indexes:further-education:Name"] = "FE_INDEX_NAME",
        };

        builder.WithConfiguration(searchIndexStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithAzureSearchConnectionOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string> searchConnectionStubConfig = new()
        {
            // AzureSearchConnectionOptions: Connection details for Azure Search
            ["AzureSearchConnectionOptions:EndpointUri"] = "https://localhost:8443",
            ["AzureSearchConnectionOptions:Credentials"] = "SEFSOFOIWSJFSO"
        };

        builder.WithConfiguration(searchConnectionStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithAzureSearchOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string> azureSearchConnectionStubConfig = new()
        {
            // AzureSearchOptions: Parameters controlling search behavior
            ["AzureSearchOptions:SearchIndex"] = "FE_INDEX_NAME",
            ["AzureSearchOptions:SearchMode"] = "0",                // Typically represents 'Any' or 'All'
            ["AzureSearchOptions:Size"] = "40000",                  // Max number of results
            ["AzureSearchOptions:IncludeTotalCount"] = "true",      // Whether to include result count
        };

        builder.WithConfiguration(azureSearchConnectionStubConfig);

        return builder;
    }

    public static IConfigurationBuilder WithSearchCriteriaOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string> searchCriteriaStub = new()
        {
            // SearchCriteria: Fields and facets used in search queries
            ["SearchCriteria:SearchFields:0"] = "Forename",
            ["SearchCriteria:SearchFields:1"] = "Surname",
            ["SearchCriteria:Facets:0"] = "ForenameLC",
            ["SearchCriteria:Facets:1"] = "SurnameLC",
            ["SearchCriteria:Facets:2"] = "Gender",
            ["SearchCriteria:Facets:3"] = "Sex",
        };

        builder.WithConfiguration(searchCriteriaStub);

        return builder;
    }

    public static IConfigurationBuilder WithLocalCosmosDbOptions(this IConfigurationBuilder builder)
    {
        CosmosDbOptions options = CosmosDbOptionsProvider.DefaultLocalOptions();

        Dictionary<string, string> configurationOptions = new()
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


        builder.WithConfiguration(configurationOptions);
        return builder;
    }

    public static IConfigurationBuilder WithDsiOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string> dsiStubConfig = new()
        {
            ["DsiClientId"] = "test-client-id",
            ["DsiClientSecret"] = "client_secret",
            ["DsiMetadataAddress"] = "https://integrationtest.example",
            ["DsiRedirectUrlAfterSignout"] = "REDIRECT_URL",
            ["DsiServiceId"] = "SERVICE_ID",
        };

        builder.WithConfiguration(dsiStubConfig);
        return builder;
    }
}
