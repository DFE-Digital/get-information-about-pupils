using Azure;
using Azure.Search.Documents;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Options;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Search;

public static class SearchClientServiceCollectionExtensions
{
    public static IServiceCollection AddSearchClients(
        this IServiceCollection services)
    {
        services
            .AddSearchClient("npd")
            .AddSearchClient("pupil-premium")
            .AddSingleton<ISearchClientProvider, SearchClientProvider>();

        return services;
    }

    private static IServiceCollection AddSearchClient(this IServiceCollection services, string searchClientKey)
    {
        services.AddSingleton(sp =>
        {
            AzureSearchConnectionOptions connectionOptions = sp.GetRequiredService<IOptions<AzureSearchConnectionOptions>>().Value;
            AzureSearchOptions indexOptions = sp.GetRequiredService<IOptions<AzureSearchOptions>>().Value;

            AzureSearchIndexOptions searchIndexOptions = indexOptions.GetIndexOptions(searchClientKey);

            return new SearchClient(
                endpoint: new Uri(connectionOptions.EndpointUri!),
                indexName: searchIndexOptions.SearchIndex,
                credential: new AzureKeyCredential(connectionOptions.Credentials!));
        });

        return services;
    }
}
