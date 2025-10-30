using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Client;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Factory;
using DfE.GIAP.SharedTests.Infrastructure.WireMock.Options;
using DfE.GIAP.SharedTests.TestDoubles;

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

    public async Task<List<AzureIndexEntity>> StubIndex(string indexName, IEnumerable<AzureIndexEntity>? values = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        List<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values.ToList();
        await _searchIndex.StubIndexSearchResponse(indexName, azureIndexDtos);
        return azureIndexDtos;
    }

    public void Dispose()
    {
        _wireMockClient?.Dispose();
    }
}
