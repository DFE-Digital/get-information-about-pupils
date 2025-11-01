using System.Net;
using DfE.GIAP.SharedTests.Infrastructure.SearchIndex.Endpoints;
using DfE.GIAP.SharedTests.Infrastructure.WireMock;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Client;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

internal sealed class AzureSearchIndexClient
{
    private readonly IWireMockClient _wireMockClient;

    public AzureSearchIndexClient(IWireMockClient wireMockClient)
    {
        Guard.ThrowIfNull(wireMockClient, nameof(wireMockClient));
        _wireMockClient = wireMockClient;
    }

    public async Task StubIndexListResponse(params string[] indexNames)
    {
        AzureSearchGetIndexesResponseDto dto = new()
        {
            value = indexNames.Select((index) => new SearchIndexResponseDto
            {
                name = index
            })
        };

        RequestMatch request = new(
            path: "/indexes",
            method: HttpMethod.Get,
            queryParams: [
                new("api-version", "2025-09-01"),
                new("$select", "name")
            ]);

        Response<AzureSearchGetIndexesResponseDto> response = new(HttpStatusCode.OK, dto);

        await _wireMockClient.Stub(request, response);
    }

    public async Task StubIndexSearchResponse(
        string indexName,
        IEnumerable<object> indexDocuments)
    {
        AzureSearchPostSearchIndexResponseDto dto = new()
        {
            value = indexDocuments
        };

        RequestMatch request = new(
            path: $"/indexes('{indexName}')/docs/search.post.search",
            method: HttpMethod.Post,
            queryParams: [
                new("api-version", "2025-09-01")
            ]);

        Response<AzureSearchPostSearchIndexResponseDto> response = new(HttpStatusCode.OK, dto);

        await _wireMockClient.Stub(request, response);
    }
}
