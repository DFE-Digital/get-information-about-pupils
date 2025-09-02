using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
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
            Port = result.Port,
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
