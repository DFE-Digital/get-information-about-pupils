using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.IndexNames.Providers;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Options;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Providers;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

/// <summary>
/// Mirrors <c>SearchByKeywordClientProvider</c> from
/// <c>Dfe.Data.Common.Infrastructure.CognitiveSearch</c>, including its per-index client caching and
/// its failure mode for an unconfigured index.
/// <para>
/// The single difference is that the <see cref="SearchClient"/> is given a
/// <see cref="SearchClientOptions"/> whose transport is backed by <see cref="AzureSearchIndexStub"/>.
/// The package's own provider offers no way to supply those options, but it is registered with
/// <c>TryAddSingleton</c>, so registering this implementation first leaves it in place.
/// </para>
/// </summary>
internal sealed class StubSearchByKeywordClientProvider : ISearchByKeywordClientProvider
{
    private readonly Dictionary<string, Lazy<SearchClient>> _lazySearchClients;

    public StubSearchByKeywordClientProvider(
        IOptions<AzureSearchConnectionOptions> azureSearchOptions,
        ISearchIndexNamesProvider indexNamesProvider,
        AzureSearchIndexStub stub)
    {
        ArgumentNullException.ThrowIfNull(azureSearchOptions);
        ArgumentNullException.ThrowIfNull(indexNamesProvider);
        ArgumentNullException.ThrowIfNull(stub);

        AzureSearchConnectionOptions connectionOptions = azureSearchOptions.Value;
        HttpMessageHandler handler = stub.CreateHandler();

        _lazySearchClients = indexNamesProvider
            .GetIndexNames()
            .ToDictionary(
                indexName => indexName,
                indexName => new Lazy<SearchClient>(
                    () => CreateSearchClientInstance(connectionOptions, indexName, handler)));
    }

    public Task<SearchClient> InvokeSearchClientAsync(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return _lazySearchClients.TryGetValue(indexName, out Lazy<SearchClient>? searchClient)
            ? Task.FromResult(searchClient.Value)
            : throw new SearchByKeywordClientInvocationException(indexName);
    }

    private static SearchClient CreateSearchClientInstance(
        AzureSearchConnectionOptions connectionOptions,
        string indexName,
        HttpMessageHandler handler)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionOptions.EndpointUri);
        ArgumentException.ThrowIfNullOrEmpty(connectionOptions.Credentials);

        SearchClientOptions searchClientOptions = new()
        {
            Transport = new HttpClientTransport(handler)
        };

        // A miss against the stub returns 404; without this the SDK retries it with backoff.
        searchClientOptions.Retry.MaxRetries = 0;

        return new SearchClient(
            new Uri(connectionOptions.EndpointUri),
            indexName,
            new AzureKeyCredential(connectionOptions.Credentials),
            searchClientOptions);
    }
}
