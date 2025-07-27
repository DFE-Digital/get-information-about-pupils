using DfE.GIAP.Core.Common;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.SharedTests;
public static class CompositionRoot
{
    public static IServiceCollection AddSharedTestDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // These are provided by the runtime; Logging, Configuration etc. Registered types will fail if they depend on these these. We stub and configure them
        services
            .AddFeaturesSharedDependencies()
            .AddLocalConfiguration()
            .AddInMemoryLogger();

        return services;
    }

    private static IServiceCollection AddLocalConfiguration(this IServiceCollection services)
    {
        Dictionary<string, string> contentConfiguration = new()
        {
            // PageContentOptions
            ["PageContentOptions:Content:TestPage1:0:Key"] = "TestContentKey1",

            // ContentRepositoryOptions
            ["ContentRepositoryOptions:ContentKeyToDocumentMapping:TestContentKey1:DocumentId"] = "DocumentId1",

            // SearchIndexOptions
            ["SearchIndexOptions:Url"] = "https://localhost:44444",
            ["SearchIndexOptions:Key"] = "SEFSOFOIWSJFSO",
            ["SearchIndexOptions:IndexOptions:0:Name"] = "npd",
            ["SearchIndexOptions:IndexOptions:0:IndexName"] = "npd-index",
            ["SearchIndexOptions:IndexOptions:1:Name"] = "pupil-premium",
            ["SearchIndexOptions:IndexOptions:1:IndexName"] = "pupil-premium-index",
        };

        IConfiguration configuration = ConfigurationTestDoubles.Default()
            .WithLocalCosmosDb()
            .WithConfiguration(contentConfiguration)
            .Build();

        services.AddSingleton(configuration);

        return services;
    }

    private static IServiceCollection AddInMemoryLogger(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));
        return services;
    }
}
