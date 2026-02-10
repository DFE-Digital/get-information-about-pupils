namespace DfE.GIAP.Core.Search.Application.Options.Search;

public sealed class SearchIndexOptionsProvider : ISearchIndexOptionsProvider
{
    private readonly SearchOptions _searchOptions;

    public SearchIndexOptionsProvider(SearchOptions searchOptions)
    {
        ArgumentNullException.ThrowIfNull(searchOptions);
        _searchOptions = searchOptions;
    }

    public SearchIndexOptions GetOptions(string key)
    {

        if (_searchOptions.Indexes is null)
        {
            throw new ArgumentException("Indexes cannot be keyed as null");
        }

        if (!_searchOptions.Indexes.TryGetValue(key, out SearchIndexOptions? options))
        {
            throw new ArgumentException($"Unable to find key {key} in SearchOptions");
        };

        if (options is null)
        {
            throw new ArgumentException($"SearchOptions cannot be null for key {key}");
        }

        return options!;
    }
}
