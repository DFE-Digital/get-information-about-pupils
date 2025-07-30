using Azure.Search.Documents;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Options.Extensions;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Client;
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
    public SearchClient GetClientByKey(string name)
    {
        string indexNameFromOptions = _searchOptions.GetIndexOptionsByName(name).IndexName;
        return _searchClients.Single(
            (t) => t.IndexName == indexNameFromOptions);
    }
}
