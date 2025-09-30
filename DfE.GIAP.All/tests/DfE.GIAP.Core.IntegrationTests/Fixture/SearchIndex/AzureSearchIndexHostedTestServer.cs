using System.Security.Cryptography.X509Certificates;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;

/// <summary>
/// A test fixture that hosts an in-memory WireMock server to simulate Azure Cognitive Search.
/// Provides stubs for index listing and search endpoints so integration tests can run without
/// hitting a real Azure Search service.
/// </summary>
internal sealed class AzureSearchIndexHostedTestServer : IDisposable
{
    private readonly WireMockServer _server;

    /// <summary>
    /// Exposes the log entries captured by WireMock (all requests/responses).
    /// Useful for debugging when a request did not match a stub.
    /// </summary>
    public IReadOnlyList<WireMock.Logging.ILogEntry> Logs => _server.LogEntries;

    /// <summary>
    /// Creates and starts a WireMock server bound to the port specified in SearchIndexOptions.
    /// </summary>
    public AzureSearchIndexHostedTestServer()
    {
        // Start WireMock server with SSL enabled (required by Azure.Search SDK clients)
        _server = WireMockServer.Start(new WireMockServerSettings
        {
            UseSSL = true,
            CertificateSettings = new WireMockCertificateSettings
            {
                X509Certificate = new X509Certificate2("localhost.pfx", "yourpassword")
            },
            Port = 0 // 0 means "pick any available port"
        });
    }

    /// <summary>
    /// Returns the base URL of the WireMock server (e.g. https://localhost:5001).
    /// </summary>
    public string Url => _server.Urls[0];

    /// <summary>
    /// Stubs the Azure Search "search documents" endpoint for a given index.
    /// When the SDK posts to /indexes/{indexName}/docs/search, this stub responds
    /// with the provided set of documents.
    /// </summary>
    public void StubSearchResponseForIndex(
        string indexName,
        IEnumerable<AzureIndexEntity> indexDocuments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        // Build a fake search response payload with the supplied documents
        var indexResponse = new
        {
            value = indexDocuments?.Select(indexEntity => new
            {
                @searchScore = indexEntity.Score,
                indexEntity.id,
                indexEntity.UPN,
                indexEntity.ULN,
                indexEntity.Surname,
                indexEntity.Forename,
                indexEntity.Sex,
                indexEntity.Gender,
                indexEntity.DOB,
                indexEntity.LocalAuthority
            }) ?? [],
        };

        // Register the stub: match POST to the search endpoint and return the fake response
        _server
            .Given(Request.Create()
                .WithPath($"/indexes('{indexName}')/docs/search.post.search") // NOTE: adjust if SDK calls a different path
                .UsingPost())
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonConvert.SerializeObject(indexResponse))
                .WithStatusCode(200));
    }

    /// <summary>
    /// Stubs the Azure Search "list indexes" endpoint.
    /// When the SDK calls GET /indexes?api-version=..., this stub responds with the given index names.
    /// </summary>
    public void StubIndexListResponse(params string[] indexNames)
    {
        _server
            .Given(Request.Create()
                .WithPath("/indexes")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonConvert.SerializeObject(new
                {
                    value = indexNames.Select(name => new { name })
                }))
                .WithStatusCode(200));
    }

    /// <summary>
    /// Clean up the WireMock server when the fixture is disposed.
    /// </summary>
    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }
}
