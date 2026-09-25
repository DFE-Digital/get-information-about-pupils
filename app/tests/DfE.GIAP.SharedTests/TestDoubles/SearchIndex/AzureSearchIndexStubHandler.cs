using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

/// <summary>
/// Serves <see cref="AzureSearchIndexStub"/> responses directly from memory, standing in for the
/// transport of the Azure Search SDK.
/// </summary>
internal sealed class AzureSearchIndexStubHandler : HttpMessageHandler
{
    // Azure Search routes document queries as /indexes('{index}')/docs/search.post.search
    private static readonly Regex s_searchRoutePattern = new(
        @"^/indexes\('(?<index>[^']+)'\)/docs/search\.post\.search$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly AzureSearchIndexStub _stub;

    public AzureSearchIndexStubHandler(AzureSearchIndexStub stub)
    {
        ArgumentNullException.ThrowIfNull(stub);
        _stub = stub;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Uri requestUri = request.RequestUri
            ?? throw new InvalidOperationException("Search index request was made without a request URI.");

        Match route = s_searchRoutePattern.Match(Uri.UnescapeDataString(requestUri.AbsolutePath));
        string? indexName = route.Success ? route.Groups["index"].Value : null;

        string body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _stub.Record(new RecordedSearchRequest(request.Method, requestUri, indexName, body));

        if (indexName is null || !_stub.TryGetResponseBody(indexName, out string? responseBody))
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                RequestMessage = request,
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
            };
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            RequestMessage = request,
            Content = new StringContent(responseBody!, Encoding.UTF8, "application/json")
        };
    }
}
