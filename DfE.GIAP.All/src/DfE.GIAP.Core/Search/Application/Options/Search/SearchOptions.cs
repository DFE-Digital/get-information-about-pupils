using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Search.Options.Search;

public sealed class SearchOptions
{
    [JsonProperty(nameof(Indexes))]
    public Dictionary<string, SearchIndexOptions>? Indexes { get; set; }
}
