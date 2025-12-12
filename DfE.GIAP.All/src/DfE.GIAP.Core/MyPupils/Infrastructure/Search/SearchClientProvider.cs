using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Infrastructure.Options;
using Microsoft.Extensions.Options;
using DfE.GIAP.Core.Search.Infrastructure.Options.Extensions;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Search;
internal sealed class SearchClientProvider : ISearchClientProvider
{
    private readonly IReadOnlyList<SearchClient> _searchClients;
    private readonly AzureSearchOptions _options;

    public SearchClientProvider(
        IEnumerable<SearchClient> searchClients,
        IOptions<AzureSearchOptions> options)
    {
        if (searchClients is null || !searchClients.Any())
        {
            throw new ArgumentException("Search clients cannot be null or empty");
        }

        _searchClients = searchClients.ToList().AsReadOnly();

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _options = options.Value;
    }

    public async Task<List<TResult>> InvokeSearchAsync<TResult>(
        string clientKey,
        SearchOptions options)
    {
        return await InvokeClientByKeyAsync(clientKey, async client =>
        {
            Azure.Response<SearchResults<TResult>> results = await client.SearchAsync<TResult>("*", options);
            List<TResult> output = [];

            await foreach (SearchResult<TResult> result in results.Value.GetResultsAsync())
            {
                output.Add(result.Document);
            }

            return output;
        });
    }

    private Task<TResult> InvokeClientByKeyAsync<TResult>(
        string key,
        Func<SearchClient, Task<TResult>> action)
    {
        SearchClient client = GetClientByKey(key);
        return action(client);
    }

    private SearchClient GetClientByKey(string clientKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientKey);

        string indexName = _options.GetIndexOptions(clientKey).SearchIndex;
        return _searchClients.Single((t) => t.IndexName == indexName);
    }
}
