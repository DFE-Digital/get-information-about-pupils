using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.SharedTests.Runtime;

public static class ServiceCollectionCompositionRoot
{
    // These are provided by the runtime; Logging, Configuration etc. Resolving types will fail without these as they are dependant on them
    public static IServiceCollection AddAspNetCoreRuntimeProvidedServices(this IServiceCollection services, IConfiguration? customConfiguration = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));

        IConfigurationBuilder builder =
            ConfigurationTestDoubles
                .DefaultConfigurationBuilder()
                .WithLocalCosmosDbOptions();

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
