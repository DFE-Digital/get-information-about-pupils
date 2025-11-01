using DfE.GIAP.SharedTests.Infrastructure.WireMock.Client;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Factory;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Options;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

public sealed class SearchIndexFixture : IDisposable
{
    private readonly IWireMockClient _wireMockClient;
    private readonly AzureSearchIndexClient _searchIndex;

    // TODO Registering json bundle on disk to /__admin/mappings - Rather than programatically generating - could we pass this as Configuration to enable this with a 'default'??
    // TODO expose ClearStubs
    public SearchIndexFixture()
    {
        WireMockServerOptions options = new()
        {
            ServerMode = WireMockServerMode.Remote,
            Domain = "localhost",
            Port = 8443,
            EnableSecureConnection = true,
            CertificatePassword = "yourpassword",
            CertificatePath = "wiremock-cert.pfx",
        };

        _wireMockClient = WireMockClientFactory.Create(options);
        _searchIndex = new AzureSearchIndexClient(_wireMockClient);
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

    public void Dispose()
    {
        _wireMockClient?.Dispose();
    }
}
