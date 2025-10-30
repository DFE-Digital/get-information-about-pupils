using System.Text;
using Newtonsoft.Json;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock;
internal sealed class WireMockRemoteClient : IWireMockClient
{
    // TODO RestClient shipped from WireMock meaning no need to manage and dispose of HttpClient
    private readonly HttpClient _httpClient;

    public WireMockRemoteClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    public async Task Stub<TDataTransferObject>(RequestMatch request, Response<TDataTransferObject> response)
    {
        // TODO use a WireMock type from WireMock library than anon object serialise
        var stub = new
        {
            request = new
            {
                method = request.Method,
                url = request.PathAndQueryString
            },
            response = new
            {
                status = response.StatusCode,
                headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json"
                },
                jsonBody = response.Body
            }
        };

        StringContent content = new(
            content: JsonConvert.SerializeObject(stub),
            encoding: Encoding.UTF8,
            mediaType: "application/json");

        await _httpClient.PostAsync("/__admin/mappings", content);
    }
}
