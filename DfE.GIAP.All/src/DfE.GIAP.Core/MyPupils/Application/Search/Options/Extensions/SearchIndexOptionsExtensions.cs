namespace DfE.GIAP.Core.MyPupils.Application.Search.Options.Extensions;
public static class SearchIndexOptionsExtensions
{
    public static IndexOptions GetIndexOptionsByName(this SearchIndexOptions searchIndexOptions, string name)
    {
        IndexOptions? options =
            searchIndexOptions.IndexOptions.SingleOrDefault(
                t => t.Name == name);

        ArgumentNullException.ThrowIfNull(options);
        return options;
    }
}
