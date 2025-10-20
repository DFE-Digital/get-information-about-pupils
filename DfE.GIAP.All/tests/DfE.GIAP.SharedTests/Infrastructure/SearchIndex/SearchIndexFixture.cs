using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;
public sealed class SearchIndexFixture : IDisposable
{
    private readonly AzureSearchIndexServer _server;
    private readonly SearchIndexOptions _options;

    public SearchIndexFixture(IOptions<SearchIndexOptions> options)
    {
        _server = new AzureSearchIndexServer(options);
        _options = options.Value;
    }

    private IndexOptions NpdIndexOptions => _options.GetIndexOptionsByName("npd");
    private IndexOptions PupilPremiumIndexOptions => _options.GetIndexOptionsByName("pupil-premium");

    public void Dispose()
    {
        _server?.Dispose();
    }

    public async Task<IEnumerable<AzureIndexEntity>> StubNpdSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values;
        await _server.StubSearchResponseForIndex(NpdIndexOptions.Name, azureIndexDtos);
        return azureIndexDtos;
    }


    public async Task<IEnumerable<AzureIndexEntity>> StubPupilPremiumSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexEntityDtosTestDoubles.Generate() : values;
        await _server.StubSearchResponseForIndex(PupilPremiumIndexOptions.Name, azureIndexDtos);

        return azureIndexDtos;
    }
}
