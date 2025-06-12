using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.SharedTests;
public static class CompositionRoot
{
    public static IServiceCollection AddSharedDependencies(this IServiceCollection services)
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
            // RepositoryOptions
            ["ContentRepositoryOptions:ContentKeyToDocumentMapping:TestContentKey1:DocumentId"] = "DocumentId1",
        };

        IConfiguration configuration = ConfigurationTestDoubles.Default()
                .WithLocalCosmosDb()
                .WithConfiguration(contentConfiguration)
                .Build();

        services.AddSingleton<IConfiguration>(configuration);

        return services;
    }
}
