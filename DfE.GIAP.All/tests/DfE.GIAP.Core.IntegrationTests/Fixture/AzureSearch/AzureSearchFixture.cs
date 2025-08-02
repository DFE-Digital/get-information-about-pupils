using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WireMock.Server;
using WireMock.Settings;
using Request = WireMock.RequestBuilders.Request;


namespace DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
internal sealed class AzureSearchMockFixture : IDisposable
{
    private readonly AzureSearchIndexHostedTestServer _server;
    private readonly SearchIndexOptions _options;

    public AzureSearchMockFixture(IOptions<SearchIndexOptions> options)
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

internal sealed class AzureSearchIndexHostedTestServer : IDisposable
{
    private readonly WireMockServer _server;

    public AzureSearchIndexHostedTestServer(IOptions<SearchIndexOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);

        if (!Uri.TryCreate(options.Value.Url, uriKind: UriKind.Absolute, out Uri? result))
        {
            throw new ArgumentException($"Unable to create Search Mock fixture with Url {options.Value.Url}");
        }

        _server = WireMockServer.Start(new WireMockServerSettings
        {
            UseSSL = true, // required for connections through Azure.Search.SearchClient
            Port = result.Port
        });
    }
    public string Url => _server.Urls[0];

    public void StubSearchResponseForIndex(
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

        _server
            .Given(Request.Create()
                .WithPath($"/indexes('{indexName}')/docs/search.post.search")
                .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonConvert.SerializeObject(indexResponse))
                .WithStatusCode(200));
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }
}
