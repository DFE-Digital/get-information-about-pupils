using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

public sealed class SearchIndexFixture : IDisposable
{
    private readonly AzureSearchIndexHttpClient _client;

    public SearchIndexFixture()
    {
        _client = new();
    }

    public void Dispose() => _client.Dispose();

    public async Task<string[]> StubAvailableIndexes(params string[] indexNames)
    {
        await _client.StubIndexListResponse(indexNames);
        return indexNames;
    }

    public async Task<List<AzureIndexEntity>> StubNpdSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        List<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values.ToList();
        await _client.StubSearchResponseForIndex(indexName: "NPD_INDEX_NAME", azureIndexDtos);
        return azureIndexDtos;
    }

    public async Task<List<AzureIndexEntity>> StubPupilPremiumSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        List<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values.ToList();
        await _client.StubSearchResponseForIndex(indexName: "PUPIL_PREMIUM_INDEX_NAME", azureIndexDtos);
        return azureIndexDtos;
    }

    public async Task<List<AzureIndexEntity>> StubFurtherEducationIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        List<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values.ToList();
        await _client.StubSearchResponseForIndex(indexName: "FE_INDEX_NAME", azureIndexDtos);
        return azureIndexDtos;
    }
}
