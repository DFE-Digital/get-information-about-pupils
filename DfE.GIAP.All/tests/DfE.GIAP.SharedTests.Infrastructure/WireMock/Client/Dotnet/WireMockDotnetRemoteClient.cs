using DfE.GIAP.SharedTests.Infrastructure.WireMock.Client;

namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Client.Dotnet;
public sealed class WireMockDotNetRemoteClient : IWireMockRemoteClient
{
    private readonly HttpClient _client;
    private readonly RemoteWireMockHostServerOptions _serverOptions;

    public WireMockDotNetRemoteClient(HttpClient client, RemoteWireMockHostServerOptions serverOptions)
    {
        Guard.ThrowIfNull(client, nameof(client));
        _client = client;

        Guard.ThrowIfNull(serverOptions, nameof(serverOptions));
        _serverOptions = serverOptions;
    }

    public async Task PostMappingsAsync(MappingModel mapping)
    {
        Guard.ThrowIfNull(mapping, nameof(mapping));

        StringContent content = new(
                content: JsonConvert.SerializeObject(mapping),
                encoding: Encoding.UTF8,
                mediaType: "application/json");

        await _client.PostAsync(_serverOptions.MappingEndpoint, content);
    }
}
