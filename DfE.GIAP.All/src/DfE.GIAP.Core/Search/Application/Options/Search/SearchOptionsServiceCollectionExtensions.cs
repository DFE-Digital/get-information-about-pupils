using DfE.GIAP.Web.Features.Search.Options.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.Search.SearchOptionsExtensions;

public static class SearchOptionsServiceCollectionExtensions
{
    public static IServiceCollection AddSearchOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<SearchOptions>()
            .Bind(configuration.GetSection(nameof(SearchOptions)));

        // Register strongly typed configuration instances.
        services.AddSingleton(
            serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<SearchOptions>>().Value);

        services.TryAddSingleton<ISearchIndexOptionsProvider, SearchIndexOptionsProvider>();

        return services;
    }
}
