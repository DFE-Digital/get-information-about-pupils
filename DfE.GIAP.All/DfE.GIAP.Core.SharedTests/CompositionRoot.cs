using DfE.GIAP.Core.SharedTests.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.SharedTests;
public static class CompositionRoot
{
    public static IServiceCollection AddTestServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
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
        services.AddSingleton<IConfiguration>(
            ConfigurationTestDoubles.WithLocalCosmosDbRepositoryOptions());
        return services;
    }
}
