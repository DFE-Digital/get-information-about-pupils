using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Web.Features.Search.Options.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Web.Features.Search.SearchOptionsExtensions;

public static class SearchOptionsServiceCollectionExtensions
{
    // Manually bound as Default Options Binder limitations when working with nested keys in Dictionaries; treats them all as flat bindable properties
    public static IServiceCollection AddSearchOptions(this IServiceCollection services, IConfiguration configuration)
    {
        IConfiguration indexesSection = configuration.GetSection("SearchOptions:Indexes");

        SearchOptions options = new()
        {
            Indexes = []
        };

        foreach (IConfigurationSection child in indexesSection.GetChildren())
        {
            SearchIndexOptions entry = new();
            child.Bind(entry);
            options.Indexes.Add(child.Key, entry);
        }

        services.AddSingleton(options);
        services.TryAddSingleton<ISearchIndexOptionsProvider, SearchIndexOptionsProvider>();

        return services;
    }

}
