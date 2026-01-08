using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Search;
internal sealed class SearchClientProvider : ISearchClientProvider
{
    private readonly SearchIndexOptions _searchOptions;
    private readonly IEnumerable<SearchClient> _searchClients;

    public SearchClientProvider(
        IEnumerable<SearchClient> searchClients,
        IOptions<SearchIndexOptions> searchOptions)
    {
        if (!searchClients.Any())
        {
            throw new ArgumentException("No search clients registered");
        }
        _searchClients = searchClients;
        _searchOptions = searchOptions.Value;
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

        string indexNameFromOptions = _searchOptions.GetIndexOptionsByName(clientKey).Name;
        return _searchClients.Single((t) => t.IndexName == indexNameFromOptions);
    }
}
