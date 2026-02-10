using Newtonsoft.Json;

namespace DfE.GIAP.Core.Search.Application.Options.Search;

public sealed class SearchOptions
{
    [JsonProperty(nameof(Indexes))]
    public Dictionary<string, SearchIndexOptions>? Indexes { get; set; }
}
