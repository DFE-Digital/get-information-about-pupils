using System.Security.Cryptography.X509Certificates;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Options;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Client;

internal sealed class WireMockLocalClient : IWireMockClient
{
    private readonly WireMockServer _wireMockServer;

    public WireMockLocalClient(WireMockServerOptions serverOptions)
    {
        WireMockCertificateSettings? cert =
            string.IsNullOrEmpty(serverOptions.CertificatePath) ?
                null :
                new WireMockCertificateSettings()
                {
                    X509Certificate = new X509Certificate2(serverOptions.CertificatePath, serverOptions.CertificatePassword ?? string.Empty)
                };

        _wireMockServer = WireMockServer.Start(new WireMockServerSettings
        {
            Port = serverOptions.Port,
            CertificateSettings = cert,
            UseSSL = serverOptions.EnableSecureConnection
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
