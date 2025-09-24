using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.SharedTests;

public static class CompositionRoot
{
    // These are provided by the runtime; Logging, Configuration etc. Resolving types will fail without these as they are dependant on them
    public static IServiceCollection AddSharedTestDependencies(this IServiceCollection services, Dictionary<string, string> extendedConfiguration = default!)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddCosmosDbDependencies()
            .AddFeaturesSharedDependencies()
            .AddLocalConfiguration(extendedConfiguration)
            .AddInMemoryLogger();

        return services;
    }

    private static IServiceCollection AddInMemoryLogger(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));
        return services;
    }

    public static IServiceCollection AddLocalConfiguration(this IServiceCollection services, Dictionary<string, string> extendedConfiguration)
    {
        Dictionary<string, string> contentConfiguration = new()
        {
            // PageContentOptions
            ["PageContentOptions:Content:TestPage1:0:Key"] = "TestContentKey1",

            // ContentRepositoryOptions
            ["ContentRepositoryOptions:ContentKeyToDocumentMapping:TestContentKey1:DocumentId"] = "DocumentId1",

            //// SearchIndexOptions
            //["SearchIndexOptions:Url"] = searchServiceUrl,
            //["SearchIndexOptions:Key"] = "SEFSOFOIWSJFSO",
            //["SearchIndexOptions:Indexes:npd:Name"] = "npd",
            //["SearchIndexOptions:Indexes:pupil-premium:Name"] = "pupil-premium-index",
            //["SearchIndexOptions:Indexes:further-education:Name"] = "further-education",

            //// SearchCriteria
            //["SearchCriteria:SearchFields:0"] = "Forename",
            //["SearchCriteria:SearchFields:1"] = "Surname",
            //["SearchCriteria:Facets:0"] = "ForenameLC",
            //["SearchCriteria:Facets:1"] = "SurnameLC",
            //["SearchCriteria:Facets:2"] = "Gender",
            //["SearchCriteria:Facets:3"] = "Sex",

            //// AzureSearchOptions
            //["AzureSearchOptions:SearchIndex"] = "further-education",
            //["AzureSearchOptions:SearchMode"] = "0",
            //["AzureSearchOptions:Size"] = "40000",
            //["AzureSearchOptions:IncludeTotalCount"] = "true",

            //// AzureSearchConnectionOptions
            //["AzureSearchConnectionOptions:EndpointUri"] = searchServiceUrl,
            //["AzureSearchConnectionOptions:Credentials"] = "SEFSOFOIWSJFSO"
        };

        if (extendedConfiguration != default)
        {
            contentConfiguration =
                contentConfiguration.Concat(extendedConfiguration)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        

        IConfiguration configuration = ConfigurationTestDoubles.Default()
                .WithLocalCosmosDb()
                .WithConfiguration(contentConfiguration)
                .Build();

        services.AddSingleton(configuration);

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<SearchCriteria>>().Value);

        return services;
    }
}
