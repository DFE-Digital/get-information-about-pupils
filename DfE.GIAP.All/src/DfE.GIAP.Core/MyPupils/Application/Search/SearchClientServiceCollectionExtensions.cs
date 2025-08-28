using Azure;
using Azure.Search.Documents;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.MyPupils.Application.Search;

public static class SearchClientServiceCollectionExtensions
{
    public static IServiceCollection AddSearchClients(
        this IServiceCollection services,
        IEnumerable<Action<SearchClientOptions>>? configureOptions = null)
    {
        services.AddSingleton(sp =>
        {
            SearchIndexOptions options = sp.GetRequiredService<IOptions<SearchIndexOptions>>().Value;
            SearchClientOptions clientOptions = new();

            configureOptions?.ToList()
                .ForEach((configure)
                    => configure?.Invoke(clientOptions));

            return new SearchClient(
                new Uri(options.Url),
                options.GetIndexOptionsByName("npd").Name,
                new AzureKeyCredential(options.Key),
                clientOptions);
        });

        services.AddSingleton(sp =>
        {
            SearchIndexOptions options = sp.GetRequiredService<IOptions<SearchIndexOptions>>().Value;
            SearchClientOptions clientOptions = new();

            configureOptions?.ToList()
                .ForEach((configure)
                    => configure?.Invoke(clientOptions));

            return new SearchClient(
                new Uri(options.Url),
                options.GetIndexOptionsByName("pupil-premium").Name,
                new AzureKeyCredential(options.Key),
                clientOptions);
        });

        return services;
    }
}
