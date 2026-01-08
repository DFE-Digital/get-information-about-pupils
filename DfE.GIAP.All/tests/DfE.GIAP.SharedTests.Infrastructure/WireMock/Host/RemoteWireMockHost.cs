namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Server;

internal class RemoteWireMockHost : IWireMockHost
{
    private readonly IWireMockRemoteClient _client;
    private readonly RemoteWireMockHostServerOptions _serverOptions;

    private bool _serverStarted = false;

    public RemoteWireMockHost(RemoteWireMockHostServerOptions serverOptions, IWireMockRemoteClient client)
    {
        Guard.ThrowIfNull(serverOptions, nameof(serverOptions));
        _serverOptions = serverOptions;

        Guard.ThrowIfNull(client, nameof(client));
        _client = client;
    }

    public Uri? Endpoint => !_serverStarted ? null : _serverOptions.Endpoint;

    // TODO consider HealthCheck on IWireMockRemoteClient
    public Task StartAsync()
    {
        _serverStarted = true;
        return Task.CompletedTask;
    }

    public Task RegisterMappingAsync(MappingRequest mapping) => _client.PostMappingsAsync(mapping.Mapping);

    public async Task RegisterMappingsAsync(IEnumerable<MappingRequest> mappings)
    {
        foreach (MappingRequest mapping in mappings)
        {
            await RegisterMappingAsync(mapping);
        }
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
            // TODO ClearMappings and terminate server
        }
    }
}
