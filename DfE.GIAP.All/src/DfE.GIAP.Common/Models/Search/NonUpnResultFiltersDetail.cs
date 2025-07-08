using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class NonUpnResultFiltersDetail
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

    }
}
