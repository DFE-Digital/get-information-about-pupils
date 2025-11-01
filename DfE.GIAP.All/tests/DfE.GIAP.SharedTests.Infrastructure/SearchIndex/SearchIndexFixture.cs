using DfE.GIAP.SharedTests.Infrastructure.WireMock.Options;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Server.Host;
using Xunit;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

public sealed class SearchIndexFixture : IAsyncLifetime
{
    private readonly IWireMockServerHost _wireMockServerHost;
    private readonly AzureSearchIndexStubClient _searchIndex;

    // TODO Registering json bundle on disk to /__admin/mappings - Rather than programatically generating - could we pass this as Configuration to enable this with a 'default'??
    // TODO expose ClearStubs
    public SearchIndexFixture()
    {
        WireMockServerOptions options = new()
        {
            ServerMode = WireMockServerMode.Remote,
            EnableLazyInitialiseServer = true,
            Domain = "localhost",
            Port = 8443,
            EnableSecureConnection = true,
            CertificatePassword = "yourpassword",
            CertificatePath = "wiremock-cert.pfx",
        };

        _wireMockServerHost = WireMockServerHostFactory.Create(options);
        _searchIndex = new AzureSearchIndexStubClient(wireMockClient: _wireMockServerHost.CreateClient());
    }

    public async Task<string[]> StubAvailableIndexes(params string[] indexNames)
    {
        await _searchIndex.StubIndexListResponse(indexNames);
        return indexNames;
    }

    public async Task StubIndex(string indexName, IEnumerable<object>? values = null)
    {
        Guard.ThrowIfNullOrWhiteSpace(indexName, nameof(indexName));
        List<object> azureIndexDtos = values is null ? [] : values.ToList();
        await _searchIndex.StubIndexSearchResponse(indexName, azureIndexDtos);
    }

    public async Task InitializeAsync()
    {
        await _wireMockServerHost.StartAsync();
    }

    public Task DisposeAsync()
    {
        _wireMockServerHost?.Dispose();
        return Task.CompletedTask;
    }
}
