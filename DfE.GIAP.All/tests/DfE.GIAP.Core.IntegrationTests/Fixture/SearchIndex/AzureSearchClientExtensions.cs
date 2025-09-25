using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
internal static class AzureSearchClientExtensions
{
    internal static SearchClient WithDisabledTlsValidation(this SearchClient original)
    {
        SearchClientOptions insecureOptions = new()
        {
            Transport =
                new HttpClientTransport(
                    new HttpClient(
                        new HttpClientHandler
                        {
                            // Override SSL certificate validation — this bypasses all certificate checks
                            // WARNING: This disables security checks and should only be used in test environments
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        }))
        };

        return new SearchClient(
            original.Endpoint,
            original.IndexName,
            new AzureKeyCredential("original.Credential"),
            insecureOptions);
    }
}
