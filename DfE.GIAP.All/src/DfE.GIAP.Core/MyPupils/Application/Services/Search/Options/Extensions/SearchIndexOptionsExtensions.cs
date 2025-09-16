using DfE.GIAP.Core.MyPupils.Application.Services.Search.Options;

namespace DfE.GIAP.Core.MyPupils.Application.Services.Search.Options.Extensions;
public static class SearchIndexOptionsExtensions
{
    public static IndexOptions GetIndexOptionsByName(this SearchIndexOptions searchIndexOptions, string name)
    {
        if (!searchIndexOptions.Indexes.TryGetValue(name, out IndexOptions? value)
            || value is null)
        {
            throw new ArgumentException($"Unable to find index options for key: {name}");
        }

        return value;
    }
}
