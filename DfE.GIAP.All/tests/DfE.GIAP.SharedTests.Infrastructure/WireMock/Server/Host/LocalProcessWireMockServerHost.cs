using System.Security.Cryptography.X509Certificates;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Options;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Client;
using WireMock.Server;
using WireMock.Settings;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Host;
internal class LocalProcessWireMockServerHost : IWireMockServerHost
{
    private readonly Uri _serverUri;
    private readonly Lazy<WireMockServer> _server;
    public LocalProcessWireMockServerHost(WireMockServerOptions serverOptions)
    {
        WireMockCertificateSettings? cert =
            string.IsNullOrEmpty(serverOptions.CertificatePath) ?
                null :
                    new()
                    {
                        X509Certificate = new X509Certificate2(serverOptions.CertificatePath, serverOptions.CertificatePassword ?? string.Empty)
                    };

        _serverUri = serverOptions.ServerAddress;
        _server = CreateWireMockServer(serverOptions, cert);
    }
    public Uri Endpoint => _serverUri;

    public IWireMockStubClient CreateClient() => new WireMockLocalProcessStubClient(_server.Value);

    public Task StartAsync()
    {
        if (!_server.IsValueCreated)
        {
            _ = _server.Value;
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_server.IsValueCreated)
            {
                _server.Value.Dispose();
            }

        }
    }

    private static Lazy<WireMockServer> CreateWireMockServer(WireMockServerOptions serverOptions, WireMockCertificateSettings? cert = null)
    {
        Lazy<WireMockServer> serverFactory = new(() => WireMockServer.Start(new WireMockServerSettings
        {
            Port = serverOptions.Port,
            CertificateSettings = cert,
            UseSSL = serverOptions.EnableSecureConnection
        }));

        if (!serverOptions.EnableLazyInitialiseServer)
        {
            _ = serverFactory.Value;
        }
        return serverFactory;
    }
}
