using Newtonsoft.Json;

namespace DfE.GIAP.Core.Search.Application.Options.Search;

public sealed class SearchOptions
{
    public Dictionary<string, SearchIndexOptions>? Indexes { get; set; }
}
