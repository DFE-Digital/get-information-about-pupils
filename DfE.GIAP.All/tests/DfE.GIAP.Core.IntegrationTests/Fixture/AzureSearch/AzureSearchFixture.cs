using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
internal sealed class AzureSearchMockFixture : IDisposable
{
    public WireMockServer Server { get; }

    internal string BaseUrl => Server.Urls[0];

    public AzureSearchMockFixture(SearchIndexOptions options)
    {
        if(!Uri.TryCreate(options.Url, uriKind: UriKind.Absolute, out Uri? result))
        {
            throw new ArgumentException($"Unable to create Search Mock fixture with Url {options.Url}");
        }

        Server = WireMockServer.Start(port: result.Port);
    }

    public void StubSearchResponse(string indexName, IEnumerable<AzureIndexEntity> indexDocuments)
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
                .WithPath($"/indexes/{indexName}/docs")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonConvert.SerializeObject(indexResponse))
                .WithStatusCode(200));
    }

    public void Dispose()
    {
        Server.Stop();
        Server.Dispose();
    }
}
