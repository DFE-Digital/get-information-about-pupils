namespace DfE.GIAP.SharedTests.Infrastructure.WireMock;

public class WireMockServerFixture : IAsyncLifetime
{
    private readonly WireMockServerOptions _options;
#nullable disable
    private IWireMockHost _wireMockHost;
    private IWireMockMappingService _mappingService;
#nullable enable
    public WireMockServerFixture()
    {
        _options = new()
        {
            ServerMode = WireMockServerMode.LocalProcess,
            Domain = "localhost",
            Port = 8443,
            EnableSecureConnection = true,
            CertificatePassword = "yourpassword",
            CertificatePath = "wiremock-cert.pfx",
        };
    }

    public async Task InitializeAsync()
    {
        _wireMockHost = _options.ServerMode switch
        {
            WireMockServerMode.LocalProcess => new LocalProcessWireMockServerHost(_options),
            WireMockServerMode.Remote => await CreateRemoteHost(),
            _ => throw new NotImplementedException($"Server mode {_options.ServerMode} is not implemented."),
        };
        await _wireMockHost.StartAsync();

        _mappingService = new WireMockMappingService(_wireMockHost);
    }

    public virtual Task OnInitialiseAsync(IWireMockHost host) => Task.CompletedTask;

    public async Task<HttpMappedResponses> RegisterHttpMapping(HttpMappingRequest request)
    {
        HttpMappedResponses responses =
            await _mappingService.RegisterMappingsAsync(
                files: request.Files);

        return responses;
    }

    public async Task DisposeAsync()
    {
        using (_wireMockHost)
        {
            await OnDisposeAsync(_wireMockHost);
        }
    }

    public virtual Task OnDisposeAsync(IWireMockHost host) => Task.CompletedTask;

    private async Task<RemoteWireMockHost> CreateRemoteHost()
    {
        RemoteWireMockHostServerOptions remoteServerOptions = new(_options.ServerAddress);

        HttpClient httpClient = new()
        {
            BaseAddress = _options.ServerAddress
        };

        using HttpResponseMessage response = await httpClient.GetAsync(remoteServerOptions.MappingEndpoint);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Cannot probe WireMock server at {remoteServerOptions.MappingEndpoint}");
        }

        // Check for Kestrel ServerHeader from WireMock.NET
        bool isRemoteDotnetServer =
            response.Headers.TryGetValues("Server", out IEnumerable<string>? serverValues)
                && serverValues.Any(t => t.Contains("Kestrel"));

        IWireMockRemoteClient wireMockRemoteClient =
            isRemoteDotnetServer ?
                new WireMockDotNetRemoteClient(
                    httpClient,
                    remoteServerOptions) :
                new WireMockJavaRemoteClient(
                    httpClient,
                    remoteServerOptions,
                    new MappingModelToWireMockJavaMappingRequestMapper());

        return new RemoteWireMockHost(
            remoteServerOptions, wireMockRemoteClient);
    }
}
