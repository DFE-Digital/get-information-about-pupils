using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;
public static class ConfigurationTestDoubles
{
    public static IConfigurationBuilder Default() => new ConfigurationBuilder();

    public static IConfigurationBuilder WithConfiguration(this IConfigurationBuilder builder, Dictionary<string, string> config)
    {
        builder.Add(
            new MemoryConfigurationSource()
            {
                InitialData = config!
            });
        return builder;
    }

    public static IConfigurationBuilder WithLocalCosmosDb(this IConfigurationBuilder builder)
    {
        RepositoryOptions options = RepositoryOptionsFactory.LocalCosmosDbEmulator();
        Dictionary<string, string> configurationOptions = new()
        {
            ["RepositoryOptions:ConnectionMode"] = "1",
            ["RepositoryOptions:EndpointUri"] = options.EndpointUri,
            ["RepositoryOptions:PrimaryKey"] = options.PrimaryKey,
            ["RepositoryOptions:DatabaseId"] = options.DatabaseId,
            ["RepositoryOptions:Containers:0:application-data:ContainerName"] = "application-data",
            ["RepositoryOptions:Containers:0:application-data:PartitionKey"] = "/DOCTYPE"
        };
        builder.WithConfiguration(configurationOptions);
        return builder;
    }
}
