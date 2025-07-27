using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.SharedTests.TestDoubles;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

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


        Server = WireMockServer.Start(new WireMockServerSettings
        {
            UseSSL = true,
            Port = result.Port
        });
    }

    public IEnumerable<AzureIndexEntity> StubSearchIndexResponse(IndexOptions options)
    {
        IEnumerable<AzureIndexEntity> azureIndexDtos = AzureIndexDtosTestDoubles.Generate();
        StubSearchResponse(options, azureIndexDtos);
        return azureIndexDtos;

    }

    public void StubSearchResponse(IndexOptions options, IEnumerable<AzureIndexEntity> indexDocuments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.IndexName);
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
                .WithPath($"/indexes('{options.IndexName}')/docs/search.post.search")
                .UsingPost())
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
