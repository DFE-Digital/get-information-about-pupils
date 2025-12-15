using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.SharedTests;

public static class CompositionRoot
{
    // These are provided by the runtime; Logging, Configuration etc. Resolving types will fail without these as they are dependant on them
    public static IServiceCollection AddAspNetCoreRuntimeProvidedServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));

        return services;
    }

    public static IServiceCollection AddGiapSharedServices(this IServiceCollection services, IConfiguration? customConfiguration = null)
    {
        IConfigurationBuilder builder =
            ConfigurationTestDoubles
                .DefaultConfigurationBuilder()
                .WithLocalCosmosDbOptions();

        if(customConfiguration != null)
        {
            builder.AddConfiguration(customConfiguration);
        }

        IConfiguration config = builder.Build();

        services.RemoveAll<IConfiguration>();
        services.AddSingleton<IConfiguration>(config);

        services.AddFeaturesSharedDependencies();

        // Replace IApplicationLogger
        services.RemoveAll<IApplicationLoggerService>();
        services.AddSingleton<IApplicationLoggerService, InMemoryLoggerService>();
        return services;
    }


    public static IServiceCollection AddSharedApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        IConfiguration configuration =
            ConfigurationTestDoubles.DefaultConfigurationBuilder()
                .WithLocalCosmosDbOptions() // TODO below this are not shared dependencies, should allow client to pass their own configuration and merge in
                .WithAzureSearchConnectionOptions()
                .WithAzureSearchOptions()
                .WithSearchCriteriaOptions()
                .Build();

        services
            .AddFeaturesSharedDependencies()
            .AddAspNetCoreRuntimeProvidedServices();

        services.AddSingleton(configuration);

        return services;
    }
}
