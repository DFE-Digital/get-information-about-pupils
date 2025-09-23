using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;

/// <summary>
/// Test fixture that hosts an in-memory WireMock server simulating Azure Cognitive Search.
/// Provides convenience methods to stub index listing and search responses for specific indexes
/// (npd, pupil-premium, further-education).
/// </summary>
public sealed class SearchIndexFixture : IDisposable
{
    private readonly AzureSearchIndexHostedTestServer _server; // Underlying WireMock server wrapper
    private readonly SearchIndexOptions _options;              // Configured search index options

    /// <summary>
    /// Exposes all requests/responses captured by WireMock for debugging.
    /// Useful when a test fails due to "No matching mapping found".
    /// </summary>
    public IReadOnlyList<WireMock.Logging.ILogEntry> LogEntries => _server.Logs;

    /// <summary>
    /// Creates the fixture and starts the WireMock server on the configured port.
    /// </summary>
    public SearchIndexFixture(IOptions<SearchIndexOptions> options)
    {
        _server = new AzureSearchIndexHostedTestServer(options);
        _options = options.Value;
    }

    /// <summary>
    /// Base URL of the WireMock server (e.g. https://localhost:5001).
    /// Used by the Azure SDK clients under test.
    /// </summary>
    internal string BaseUrl => _server.Url;

    // Convenience accessors for the configured index options
    private IndexOptions NpdIndexOptions => _options.GetIndexOptionsByName("npd");
    private IndexOptions PupilPremiumIndexOptions => _options.GetIndexOptionsByName("pupil-premium");
    private IndexOptions FurtherEducationIndexOptions => _options.GetIndexOptionsByName("further-education");

    /// <summary>
    /// Dispose of the underlying WireMock server when the fixture is torn down.
    /// </summary>
    public void Dispose() => _server.Dispose();

    /// <summary>
    /// Stub the "list indexes" endpoint so the SDK sees the given indexes as available.
    /// </summary>
    public string[] StubAvailableIndexes(params string[] indexNames)
    {
        _server.StubIndexListResponse(indexNames);
        return indexNames;
    }

    /// <summary>
    /// Generic helper to stub a search index with either supplied or generated documents.
    /// </summary>
    private IEnumerable<AzureIndexEntity> StubSearchIndex(IndexOptions indexOptions, IEnumerable<AzureIndexEntity>? values = null)
    {
        // Use provided values, or generate default test doubles if none supplied
        IEnumerable<AzureIndexEntity> docs = values ?? AzureIndexEntityDtosTestDoubles.Generate();

        // Register the stub with the WireMock server
        _server.StubSearchResponseForIndex(indexOptions.Name, docs);

        return docs;
    }

    /// <summary>
    /// Stub the NPD index with either supplied or generated documents.
    /// </summary>
    public IEnumerable<AzureIndexEntity> StubNpdSearchIndex(IEnumerable<AzureIndexEntity>? values = null) =>
        StubSearchIndex(NpdIndexOptions, values);

    /// <summary>
    /// Stub the Further Education index with either supplied or generated documents.
    /// </summary>
    public IEnumerable<AzureIndexEntity> StubFurtherEducationSearchIndex(IEnumerable<AzureIndexEntity>? values = null) =>
        StubSearchIndex(FurtherEducationIndexOptions, values);

    /// <summary>
    /// Stub the Pupil Premium index with either supplied or generated documents.
    /// </summary>
    public IEnumerable<AzureIndexEntity> StubPupilPremiumSearchIndex(IEnumerable<AzureIndexEntity>? values = null) =>
        StubSearchIndex(PupilPremiumIndexOptions, values);
}
