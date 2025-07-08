using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Search.UniqueLearnerNumber
{
    [ExcludeFromCodeCoverage]
    public class NonUniqueLearnerNumberResultFilters
    {
        [JsonProperty("Gender")]
        public NonUniqueLearnerNumberResultFiltersDetail[] GenderFilters { get; set; }

        [JsonProperty("SurnameLC")]
        public NonUniqueLearnerNumberResultFiltersDetail[] SurnameFilters { get; set; }

        [JsonProperty("ForenameLC")]
        public NonUniqueLearnerNumberResultFiltersDetail[] ForenameFilters { get; set; }
    }
}
