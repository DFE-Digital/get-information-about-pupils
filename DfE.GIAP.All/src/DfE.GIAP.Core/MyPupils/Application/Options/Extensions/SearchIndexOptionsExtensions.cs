namespace DfE.GIAP.Core.MyPupils.Application.Options.Extensions;
public static class SearchIndexOptionsExtensions
{
    public static IndexOptions GetIndexOptionsByKey(this SearchIndexOptions searchIndexOptions, string key)
    {
        IndexOptions? options =
            searchIndexOptions.IndexOptions.SingleOrDefault(
                t => t.IndexName == key);

        ArgumentNullException.ThrowIfNull(options);
        return options;
    }
}
