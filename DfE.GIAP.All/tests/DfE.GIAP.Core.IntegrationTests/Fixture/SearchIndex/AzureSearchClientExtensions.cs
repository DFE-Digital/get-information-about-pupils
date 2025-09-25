using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;

internal static class AzureSearchClientExtensions
{
    internal static SearchClient WithDisabledTlsValidation(this SearchClient original)
    {
        HttpClientHandler handler = new()
        {
            // Disable SSL certificate validation — only for test harnesses
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        SearchClientOptions insecureOptions = new()
        {
            Transport = new HttpClientTransport(new HttpClient(handler))
        };

        // Use the same endpoint and index name, but hardcode the test API key
        return new SearchClient(
            original.Endpoint,
            original.IndexName,
            new AzureKeyCredential("SEFSOFOIWSJFSO"),
            insecureOptions);
    }
}
