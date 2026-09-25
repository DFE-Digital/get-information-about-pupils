using Newtonsoft.Json;

namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

/// <summary>
/// In-memory stub of the Azure Search index REST contract.
/// <para>
/// Responses are keyed by index name and served by an <see cref="HttpMessageHandler"/> that is
/// plugged into the Azure SDK via <c>SearchClientOptions.Transport</c>. No socket is opened and no
/// TLS handshake occurs, so no certificate or container is required. Everything above the transport
/// - request construction, <c>SearchOptions</c> serialisation and response deserialisation - is the
/// real SDK.
/// </para>
/// </summary>
public sealed class AzureSearchIndexStub
{
    private const string ContractsDirectory = "contracts";

    private readonly Dictionary<string, string> _responseBodiesByIndexName = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<RecordedSearchRequest> _receivedRequests = [];
    private readonly object _receivedRequestsLock = new();

    private AzureSearchIndexStub()
    {
    }

    public static AzureSearchIndexStub Create() => new();

    /// <summary>
    /// Serves the contents of <paramref name="fileName"/> (resolved under the test output
    /// <c>contracts</c> directory) for search requests against <paramref name="indexName"/>.
    /// </summary>
    public AzureSearchIndexStub WithIndexResponseFromFile(string indexName, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (Path.IsPathRooted(fileName))
        {
            throw new ArgumentException("File name must be a relative path under the contracts directory.", nameof(fileName));
        }

        string resolvedPath = Path.Combine(ContractsDirectory, fileName);

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException(
                $"Unable to resolve search index contract for index {indexName}.", resolvedPath);
        }

        _responseBodiesByIndexName[indexName] = File.ReadAllText(resolvedPath);

        return this;
    }

    /// <summary>
    /// Deserialises the body this stub will return for <paramref name="indexName"/>, so a test can
    /// derive its expectations from the same contract the application will receive.
    /// </summary>
    public TBody GetStubbedResponseFor<TBody>(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        if (!_responseBodiesByIndexName.TryGetValue(indexName, out string? body))
        {
            throw new InvalidOperationException($"No stubbed response registered for index {indexName}.");
        }

        return JsonConvert.DeserializeObject<TBody>(body)
            ?? throw new InvalidOperationException($"Failed to deserialise stubbed response for index {indexName} to {typeof(TBody).Name}.");
    }

    /// <summary>
    /// Every request the application made to the search index, in order.
    /// </summary>
    public IReadOnlyList<RecordedSearchRequest> ReceivedRequests
    {
        get
        {
            lock (_receivedRequestsLock)
            {
                return [.. _receivedRequests];
            }
        }
    }

    internal HttpMessageHandler CreateHandler() => new AzureSearchIndexStubHandler(this);

    internal bool TryGetResponseBody(string indexName, out string? body) =>
        _responseBodiesByIndexName.TryGetValue(indexName, out body);

    internal void Record(RecordedSearchRequest request)
    {
        lock (_receivedRequestsLock)
        {
            _receivedRequests.Add(request);
        }
    }
}
