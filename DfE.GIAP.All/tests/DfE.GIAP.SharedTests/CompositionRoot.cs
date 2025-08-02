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
        Dictionary<string, string> contentConfiguration = new()
        {
            // PageContentOptions
            ["PageContentOptions:Content:TestPage1:0:Key"] = "TestContentKey1",

            // ContentRepositoryOptions
            ["ContentRepositoryOptions:ContentKeyToDocumentMapping:TestContentKey1:DocumentId"] = "DocumentId1",

            // SearchIndexOptions
            ["SearchIndexOptions:Url"] = "https://localhost:44444",
            ["SearchIndexOptions:Key"] = "SEFSOFOIWSJFSO",
            ["SearchIndexOptions:Indexes:npd:Name"] = "npd",
            ["SearchIndexOptions:Indexes:pupil-premium:Name"] = "pupil-premium-index",
        };

        IConfiguration configuration = ConfigurationTestDoubles.Default()
                .WithLocalCosmosDb()
                .WithConfiguration(contentConfiguration)
                .Build();

        services.AddSingleton(configuration);

        return services;
    }
}
