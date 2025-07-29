using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.SharedTests.TestDoubles;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
internal sealed class AzureSearchMockFixture : IDisposable
{
    private readonly SearchIndexOptions _options;

    public AzureSearchMockFixture(SearchIndexOptions options)
    {
        if (!Uri.TryCreate(options.Url, uriKind: UriKind.Absolute, out Uri? result))
        {
            throw new ArgumentException($"Unable to create Search Mock fixture with Url {options.Url}");
        }


        Server = WireMockServer.Start(new WireMockServerSettings
        {
            UseSSL = true,
            Port = result.Port
        });
        _options = options;
    }

    internal WireMockServer Server { get; }
    internal string BaseUrl => Server.Urls[0];
    private IndexOptions NpdIndexOptions => _options.GetIndexOptionsByName("npd");
    private IndexOptions PupilPremiumIndexOptions => _options.GetIndexOptionsByName("pupil-premium");

    public void Dispose()
    {
        Server.Stop();
        Server.Dispose();
    }

    public IEnumerable<AzureIndexEntity> StubNpd()
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = AzureIndexDtosTestDoubles.Generate();
        StubSearchResponse( NpdIndexOptions.IndexName, azureIndexDtos);
        return azureIndexDtos;
    }

    public void StubNpd(IEnumerable<AzureIndexEntity> values) => StubSearchResponse(NpdIndexOptions.IndexName, values);

    public IEnumerable<AzureIndexEntity> StubPupilPremium()
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = AzureIndexDtosTestDoubles.Generate();
        StubSearchResponse(PupilPremiumIndexOptions.IndexName, azureIndexDtos);

        return azureIndexDtos;
    }

    public void StubPupilPremium(IEnumerable<AzureIndexEntity> values) => StubSearchResponse(PupilPremiumIndexOptions.IndexName, values);

    private void StubSearchResponse(
        string indexName,
        IEnumerable<AzureIndexEntity> indexDocuments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        var indexResponse = new
        {
            value = indexDocuments?.Select(t => new
            {
                @searchScore = t.Score,
                t.id,
                t.UPN,
                t.Surname,
                t.Forename,
                t.Sex,
                t.DOB,
                t.LocalAuthority
            }) ?? [],
        };

        Server
            .Given(Request.Create()
                .WithPath($"/indexes('{indexName}')/docs/search.post.search")
                .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonConvert.SerializeObject(indexResponse))
                .WithStatusCode(200));
    }
}
