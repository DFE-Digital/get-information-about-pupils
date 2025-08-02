using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Options;


namespace DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
internal sealed class AzureSearchFixture : IDisposable
{
    private readonly AzureSearchIndexHostedTestServer _server;
    private readonly SearchIndexOptions _options;

    public AzureSearchFixture(IOptions<SearchIndexOptions> options)
    {
        _server = new AzureSearchIndexHostedTestServer(options);
        _options = options.Value;
    }

    internal string BaseUrl => _server.Url;
    private IndexOptions NpdIndexOptions => _options.GetIndexOptionsByName("npd");
    private IndexOptions PupilPremiumIndexOptions => _options.GetIndexOptionsByName("pupil-premium");

    public void Dispose()
    {
        _server?.Dispose();
    }

    public IEnumerable<AzureIndexEntity> StubNpdSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexDtosTestDoubles.Generate() : values;
        _server.StubSearchResponseForIndex(NpdIndexOptions.Name, azureIndexDtos);
        return azureIndexDtos;
    }


    public IEnumerable<AzureIndexEntity> StubPupilPremiumSearchIndex(IEnumerable<AzureIndexEntity>? values = null)
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = values is null ? AzureIndexDtosTestDoubles.Generate() : values;
        _server.StubSearchResponseForIndex(PupilPremiumIndexOptions.Name, azureIndexDtos);

        return azureIndexDtos;
    }
}
