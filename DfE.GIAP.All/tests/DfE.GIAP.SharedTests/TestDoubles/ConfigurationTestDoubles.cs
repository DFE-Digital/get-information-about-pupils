using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class ConfigurationTestDoubles
{
    public static IConfigurationBuilder DefaultConfigurationBuilder() => new ConfigurationBuilder();

    public static IConfiguration GetTestConfiguration()
    {
        Dictionary<string, string> contentConfiguration = new()
        {
            // SearchIndexOptions
            ["SearchIndexOptions:Url"] = "https://localhost:44444",
            ["SearchIndexOptions:Key"] = "SEFSOFOIWSJFSO",
            ["SearchIndexOptions:Indexes:npd:Name"] = "npd",
            ["SearchIndexOptions:Indexes:pupil-premium:Name"] = "pupil-premium-index",

            // FeatureFlagOptions
            ["FeatureFlagAppConfigUrl"] = "Endpoint=https://featureflags.azconfig.io;Id=ID;Secret=SECRET",

            // BlobStorageOptions
            // NOTE these are not secrets just stubbed data
            ["StorageAccountName"] = "AZURE_STORAGE_ACCOUNTNAME",
            ["StorageAccountKey"] = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;",
            ["StorageContainerName"] = "AZURE_STORAGE_CONTAINERNAME",

            // DSI
            ["DsiClientId"] = "test-client-id",
            ["DsiClientSecret"] = "client_secret",
            ["DsiMetadataAddress"] = "https://integrationtest.example",
            ["DsiRedirectUrlAfterSignout"] = "REDIRECT_URL",
            ["DsiServiceId"] = "SERVICE_ID",

        };

        IConfiguration configuration = DefaultConfigurationBuilder()
                .WithLocalCosmosDb()
                .Add(new MemoryConfigurationSource()
                {
                    InitialData = contentConfiguration
                })
                .Build();

        return configuration;
    }

    public static IConfigurationBuilder WithLocalCosmosDb(this IConfigurationBuilder builder)
    {
        RepositoryOptions options = RepositoryOptionsFactory.LocalCosmosDbEmulator();

        ArgumentException.ThrowIfNullOrWhiteSpace(options.EndpointUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PrimaryKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.DatabaseId);

        Dictionary<string, string> configurationOptions = new()
        {
            ["RepositoryOptions:ConnectionMode"] = "1",
            ["RepositoryOptions:EndpointUri"] = options.EndpointUri,
            ["RepositoryOptions:PrimaryKey"] = options.PrimaryKey,
            ["RepositoryOptions:DatabaseId"] = options.DatabaseId,
            ["RepositoryOptions:Containers:0:application-data:ContainerName"] = "application-data",
            ["RepositoryOptions:Containers:0:application-data:PartitionKey"] = "/DOCTYPE",
            ["RepositoryOptions:Containers:1:news:ContainerName"] = "news",
            ["RepositoryOptions:Containers:1:news:PartitionKey"] = "/id",
            ["RepositoryOptions:Containers:2:users:ContainerName"] = "users",
            ["RepositoryOptions:Containers:2:users:PartitionKey"] = "/id"
        };
        builder.Add(new MemoryConfigurationSource()
        {
            InitialData = configurationOptions
        });
        return builder;
    }
}
