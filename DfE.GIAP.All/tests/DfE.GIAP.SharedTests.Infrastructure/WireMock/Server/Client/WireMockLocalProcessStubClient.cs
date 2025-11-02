using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Client;

internal sealed class WireMockLocalProcessStubClient : IWireMockStubClient
{
    private readonly WireMockServer _wireMockServer;

    public WireMockLocalProcessStubClient(WireMockServer wireMockServer)
    {
        Guard.ThrowIfNull(wireMockServer, nameof(wireMockServer));
        _wireMockServer = wireMockServer;
    }

    public Task Stub<TDataTransferObject>(RequestMatch request, Response<TDataTransferObject> response)
    {
        _wireMockServer
            .Given(
                Request.Create()
                    .WithPath(request.Path)
                    .UsingMethod(request.Method))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonConvert.SerializeObject(response.Body))
                    .WithStatusCode(response.StatusCode));

        return Task.CompletedTask;
    }
}
