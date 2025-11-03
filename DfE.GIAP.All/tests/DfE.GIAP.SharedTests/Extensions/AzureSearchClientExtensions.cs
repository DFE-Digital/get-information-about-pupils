using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.SharedTests.Extensions;
public static class AzureSearchClientExtensions
{
    public static SearchClient WithDisabledTlsValidation(this SearchClient original)
    {
        SearchClientOptions insecureOptions = new()
        {
            Transport =
                new HttpClientTransport(
                    new HttpClient(
                        new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        }))
        };

        return new SearchClient(
            original.Endpoint,
            original.IndexName,
            new AzureKeyCredential("original.Credential"),
            insecureOptions);
    }

    public static IServiceCollection ConfigureAzureSearchClients(this IServiceCollection services)
    {
        services.RemoveAll<ISearchClientProvider>();

        services.AddSingleton<ISearchClientProvider>(sp =>
        {
            IEnumerable<SearchClient> originalClients = sp.GetServices<SearchClient>();

            List<SearchClient> insecureClients =
                [.. originalClients.Select(client => client.WithDisabledTlsValidation())]; // Required as .NET cert store doesn't trust untrustedRoot.

            return new SearchClientProvider(
                insecureClients,
                sp.GetRequiredService<IOptions<SearchIndexOptions>>());
        });
        return services;
    }
}
