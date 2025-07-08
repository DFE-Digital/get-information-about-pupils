using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class NonUpnResultFilters
    {
        [JsonProperty("Gender")]
        public NonUpnResultFiltersDetail[] GenderFilters { get; set; }
        [JsonProperty("SurnameLC")]
        public NonUpnResultFiltersDetail[] SurnameFilters { get; set; }
        [JsonProperty("ForenameLC")]
        public NonUpnResultFiltersDetail[] ForenameFilters { get; set; }
    }
}
