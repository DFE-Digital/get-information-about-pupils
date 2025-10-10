using Azure.Search.Documents;
using DfE.GIAP.Core.MyPupils.Application.Services.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Services.Search.Provider;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
internal static class CompositionRoot
{
    internal static IServiceCollection ConfigureAzureSearchClients(this IServiceCollection services)
    {
        services.RemoveAll<ISearchClientProvider>();

        services.AddSingleton<ISearchClientProvider>(sp =>
        {
            IEnumerable<SearchClient> originalClients = sp.GetServices<SearchClient>();

            List<SearchClient> insecureClients =
                originalClients
                    .Select(client => client.WithDisabledTlsValidation()) // Required as .NET cert store doesn't trust untrustedRoot.
                    .ToList();

            return new SearchClientProvider(
                insecureClients,
                sp.GetRequiredService<IOptions<SearchIndexOptions>>());
        });
        return services;
    }
}
