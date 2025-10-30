using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock;

internal sealed class WireMockLocalClient : IWireMockClient
{
    private readonly WireMockServer _wireMockServer;

    // TODO pass server options - Certificate, Port
    public WireMockLocalClient()
    {
        _wireMockServer = WireMockServer.Start(new WireMockServerSettings
        {
            Port = 8443,
            CertificateSettings = new()
            {
                X509Certificate = new X509Certificate2("wiremock-cert.pfx", "yourpassword")
            },
            UseSSL = true
        });
    }

    public void Dispose()
    {
        using (_wireMockServer)
        {
            _wireMockServer?.Stop();
        }
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
