using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class NonUpnResults
    {
        [JsonProperty("@search.facets")]
        public NonUpnResultFilters Filters { get; set; }

        [JsonProperty("value")]
        public NonUpnResultsList[] ResultsList { get; set; }

        [JsonProperty("@odata.count")]
        public int Count { get; set; }
    }
}
