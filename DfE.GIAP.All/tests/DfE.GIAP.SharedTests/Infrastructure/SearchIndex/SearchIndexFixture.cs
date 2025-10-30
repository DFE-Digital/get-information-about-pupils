using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.Infrastructure.WireMock;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

public sealed class SearchIndexFixture : IDisposable
{
    private readonly IWireMockClient _wireMockClient;
    private readonly AzureSearchIndex _searchIndex;

    // TODO pass or read options for connection - IConfiguration from testconfiguration.json and output? Local/Remote client
    // TODO pass configuration of Index and shape in options so more flexible
    // TODO expose ClearStubs
    public SearchIndexFixture()
    {
        Uri uri = new("https://localhost:8443");

        if (!uri.IsAbsoluteUri)
        {
            throw new ArgumentException($"SearchIndex endpoint must be an absolute Uri");
        }

        HttpClient httpClient = new()
        {
            BaseAddress = uri
        };                                                          

        _wireMockClient = new WireMockRemoteClient(httpClient);
        _searchIndex = new AzureSearchIndex(_wireMockClient);
    }
    
    public async Task<string[]> StubAvailableIndexes(params string[] indexNames)
    {
        await _searchIndex.StubIndexListResponse(indexNames);
        return indexNames;
    }

    public Task<List<AzureIndexEntity>> StubNpdSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
        => StubIndex("NPD_INDEX_NAME", values);

    public Task<List<AzureIndexEntity>> StubPupilPremiumSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
        => StubIndex("PUPIL_PREMIUM_INDEX_NAME", values);

    public Task<List<AzureIndexEntity>> StubFurtherEducationIndex(IEnumerable<AzureIndexEntity>? values = null)
        => StubIndex("FE_INDEX_NAME", values);

    private async Task<List<AzureIndexEntity>> StubIndex(string indexName, IEnumerable<AzureIndexEntity>? values = null)
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
