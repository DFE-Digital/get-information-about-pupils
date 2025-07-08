using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Search.UniqueLearnerNumber
{
    [ExcludeFromCodeCoverage]
    public class NonUniqueLearnerNumberSearchResults
    {
        [JsonProperty("@search.facets")]
        public NonUniqueLearnerNumberResultFilters Filters { get; set; }

        [JsonProperty("value")]
        public NonUniqueLearnerNumberResultsList[] ResultsList { get; set; }

        [JsonProperty("@odata.count")]
        public int? Count { get; set; }
    }
}
