using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.SharedTests;
public static class CompositionRoot
{
    // These are provided by the runtime; Logging, Configuration etc. Resolving types will fail without these as they are dependant on them
    public static IServiceCollection AddSharedTestDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddFeaturesSharedDependencies()
            .AddLocalConfiguration()
            .AddInMemoryLogger();

        return services;
    }

    private static IServiceCollection AddInMemoryLogger(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));
        return services;
    }

    private static IServiceCollection AddLocalConfiguration(this IServiceCollection services)
    {

        IConfiguration configuration = ConfigurationTestDoubles.DefaultConfigurationBuilder()
                .WithLocalCosmosDbOptions()
                .WithSearchIndexOptions()
                .Build();

        services.AddSingleton(configuration);

        return services;
    }
}
