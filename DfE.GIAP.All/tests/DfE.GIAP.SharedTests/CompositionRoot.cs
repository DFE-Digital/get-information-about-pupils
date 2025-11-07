using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Configuration;

namespace DfE.GIAP.SharedTests;

public static class CompositionRoot
{
    // These are provided by the runtime; Logging, Configuration etc. Resolving types will fail without these as they are dependant on them
    public static IServiceCollection AddSharedApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        IConfiguration configuration =
            ConfigurationTestDoubles.DefaultConfigurationBuilder()
                .WithLocalCosmosDbOptions() // TODO below this are not shared dependencies, should allow client to pass their own configuration and merge in
                .WithSearchIndexOptions()
                .WithAzureSearchConnectionOptions()
                .WithAzureSearchOptions()
                .WithSearchCriteriaOptions()
                .Build();

        services
            .AddFeaturesSharedDependencies()
            .AddInMemoryLogger();

        services.AddSingleton(configuration);
        services.AddSingleton((sp) => sp.GetRequiredService<IOptions<SearchCriteria>>().Value); // TODO What uses this?

        return services;
    }

    private static IServiceCollection AddInMemoryLogger(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));
        services.AddSingleton<ILoggerService, InMemoryLoggerService>();

        return services;
    }
}
