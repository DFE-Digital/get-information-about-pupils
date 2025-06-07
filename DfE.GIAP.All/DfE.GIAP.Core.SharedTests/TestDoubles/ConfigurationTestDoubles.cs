using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;
public static class ConfigurationTestDoubles
{
    public static IConfiguration Default() => DefaultBuilder().Build();

    public static IConfiguration WithLocalCosmosDbRepositoryOptions()
    {
        ConfigurationBuilder builder = DefaultBuilder();

        RepositoryOptions repositoryOptions = RepositoryOptionsFactory.LocalCosmosDbEmulator();

        Dictionary<string, string?> config = new()
        {
            ["RepositoryOptions:ConnectionMode"] = "1",
            ["RepositoryOptions:EndpointUri"] = repositoryOptions.EndpointUri,
            ["RepositoryOptions:PrimaryKey"] = repositoryOptions.PrimaryKey,
        };

        builder.Add(
            new MemoryConfigurationSource()
            {
                InitialData = config
            });

        return builder.Build();
    }

    private static ConfigurationBuilder DefaultBuilder() => new();
}
