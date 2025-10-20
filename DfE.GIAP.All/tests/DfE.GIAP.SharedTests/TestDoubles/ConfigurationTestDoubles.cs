using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using DfE.GIAP.Core.SharedTests;
using DfE.GIAP.SharedTests.Fixtures.CosmosDb;
using DfE.GIAP.SharedTests.Fixtures.CosmosDb.Options;
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
    public static IConfigurationBuilder WithSearchIndexOptions(this IConfigurationBuilder builder)
    {
        Dictionary<string, string> searchIndexStubConfig = new()
        {
            ["SearchIndexOptions:Url"] = "https://localhost:8443",
            ["SearchIndexOptions:Key"] = "ANY_KEY",
            ["SearchIndexOptions:Indexes:npd:Name"] = "NPD_INDEX_NAME",
            ["SearchIndexOptions:Indexes:pupil-premium:Name"] = "PUPIL_PREMIUM_INDEX_NAME",
        };

        builder.WithConfiguration(searchIndexStubConfig);

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
            ["RepositoryOptions:DatabaseId"] = "giapsearch",
            ["RepositoryOptions:Containers:0:application-data:ContainerName"] = "application-data",
            ["RepositoryOptions:Containers:0:application-data:PartitionKey"] = "/DOCTYPE",
            ["RepositoryOptions:Containers:1:news:ContainerName"] = "news",
            ["RepositoryOptions:Containers:1:news:PartitionKey"] = "/id",
            ["RepositoryOptions:Containers:2:users:ContainerName"] = "users",
            ["RepositoryOptions:Containers:2:users:PartitionKey"] = "/id",
            ["RepositoryOptions:Containers:3:mypupils:ContainerName"] = "mypupils",
            ["RepositoryOptions:Containers:3:mypupils:PartitionKey"] = "/id"
        };

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
