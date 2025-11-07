using System.Text;
using Newtonsoft.Json;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Client;
internal sealed class WireMockHttpClient : IWireMockStubClient
{
    private readonly HttpClient _httpClient;

    public WireMockHttpClient(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task Stub<TDataTransferObject>(RequestMatch request, Response<TDataTransferObject> response)
    {
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
