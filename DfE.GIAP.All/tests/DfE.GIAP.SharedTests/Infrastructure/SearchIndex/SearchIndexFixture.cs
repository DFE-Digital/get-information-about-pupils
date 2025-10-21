using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

// TODO derive index stubbing from same Options from Search (MyPupils will need to be moved over)
public sealed class SearchIndexFixture : IDisposable
{
    private readonly AzureSearchIndexClient _client;          // Underlying WireMock wrapper

    // /// <summary>
    // /// Exposes all requests/responses captured by WireMock for debugging.
    // /// Useful when a test fails due to "No matching mapping found".
    // /// </summary>
    // public IReadOnlyList<WireMock.Logging.ILogEntry> LogEntries => _server.Logs;

    /// <summary>
    /// Creates the fixture and starts the WireMock server on the configured port.
    /// </summary>
    public SearchIndexFixture()
    {
        _client = new AzureSearchIndexClient();
    }

    /// <summary>
    /// Dispose of the underlying WireMock server when the fixture is torn down.
    /// </summary>
    public void Dispose() => _client.Dispose();

    /// <summary>
    /// Stub the "list indexes" endpoint so the SDK sees the given indexes as available.
    /// </summary>
    public async Task<string[]> StubAvailableIndexes(params string[] indexNames)
    {
        await _client.StubIndexListResponse(indexNames);
        return indexNames;
    }

    public async Task<IEnumerable<AzureIndexEntity>> StubNpdSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values;
        await _client.StubSearchResponseForIndex("NPD_INDEX_NAME", azureIndexDtos);
        return azureIndexDtos;
    }

    public async Task<IEnumerable<AzureIndexEntity>> StubPupilPremiumSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values;
        await _client.StubSearchResponseForIndex("PUPIL_PREMIUM_INDEX_NAME", azureIndexDtos);
        return azureIndexDtos;
    }

    public async Task<IEnumerable<AzureIndexEntity>> StubFurtherEducationIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values;
        await _client.StubSearchResponseForIndex("FE_INDEX_NAME", azureIndexDtos);
        return azureIndexDtos;
    }
}
