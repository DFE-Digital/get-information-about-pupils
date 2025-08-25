using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;
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
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        }))
        };

        return new SearchClient(
            original.Endpoint,
            original.IndexName,
            new AzureKeyCredential("original.Credential"),
            insecureOptions);
    }
}
