namespace DfE.GIAP.Core.Search.Infrastructure.Options.Extensions;
/// <summary>
/// Provides extension methods for retrieving AzureSearchIndex options from AzureSearch configuration.
/// </summary>
public static class AzureSearchIndexExtensions
{
    public static SearchIndexOptions GetIndexOptions(this AzureSearchOptions searchOptions, string searchIndexKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchIndexKey);
        ArgumentNullException.ThrowIfNull(searchOptions.Indexes);

        // Attempt to retrieve the SearchIndexOptions dictionary and fetch the requested SearchIndexOptions.
        if (!searchOptions.Indexes.TryGetValue(searchIndexKey, out SearchIndexOptions? searchIndexOptions))
        {
            throw new InvalidOperationException(
                $"SearchIndex dictionary options with index key: {searchIndexKey} not configured in options.");
        }

        return searchIndexOptions;
    }
}
