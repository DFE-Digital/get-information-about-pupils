using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.SharedTests.Runtime;

public static class ServiceCollectionCompositionRoot
{
    // Placeholders for tests that only resolve the registration graph and never open a connection.
    // The key is the emulator's well-known one purely so it parses as a valid account key.
    private const string UnusedCosmosDbEndpointUri = "http://localhost:8081";
    private const string UnusedCosmosDbPrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    // These are provided by the runtime; Logging, Configuration etc. Resolving types will fail without these as they are dependant on them
    /// <param name="cosmosDbEndpointUri">Gateway endpoint of the emulator started for this test
    /// run. The host port is assigned at container start, so it cannot be hard coded. The default
    /// is a placeholder for tests that only assert over the registration graph and never connect.</param>
    /// <param name="cosmosDbPrimaryKey">Account key the emulator accepts.</param>
    public static IServiceCollection AddAspNetCoreRuntimeProvidedServices(
        this IServiceCollection services,
        string cosmosDbEndpointUri = UnusedCosmosDbEndpointUri,
        string cosmosDbPrimaryKey = UnusedCosmosDbPrimaryKey,
        IConfiguration? customConfiguration = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));

        IConfigurationBuilder builder =
            ConfigurationTestDoubles
                .DefaultConfigurationBuilder()
                .WithLocalCosmosDbOptions(cosmosDbEndpointUri, cosmosDbPrimaryKey);

        if (customConfiguration != null)
        {
            builder.AddConfiguration(customConfiguration);
        }

        IConfiguration config = builder.Build();

        services.RemoveAll<IConfiguration>();
        services.AddSingleton(config);

        return services;
    }

    public static IServiceCollection AddFeaturesSharedServices(this IServiceCollection services)
    {
        services.AddFeaturesSharedDependencies();

        // Replace IApplicationLogger
        services.RemoveAll<IApplicationLoggerService>();
        services.AddSingleton<IApplicationLoggerService, InMemoryLoggerService>();
        return services;
    }
}
