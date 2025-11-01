using DfE.GIAP.SharedTests.Infrastructure.WireMock.Options;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Client;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Host;
internal class RemoteWireMockServerHost : IWireMockServerHost
{
    private readonly WireMockServerOptions _serverOptions;
    private readonly HttpClient _sharedServerHttpClient;

    public RemoteWireMockServerHost(WireMockServerOptions serverOptions)
    {
        _serverOptions = serverOptions;
        _sharedServerHttpClient = new HttpClient()
        {
            BaseAddress = _serverOptions.ServerAddress
        };
    }

    public Uri Endpoint => _serverOptions.ServerAddress;

    public IWireMockClient CreateClient()
    {
        WireMockRemoteClient client = new(_sharedServerHttpClient);
        return client;
    }

    // NOOP
    public Task StartAsync() => Task.CompletedTask; // TODO Could consider health check

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sharedServerHttpClient?.Dispose();
        }
    }
}
