using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Search.UniqueLearnerNumber
{
    [ExcludeFromCodeCoverage]
    public class NonUniqueLearnerNumberResultsList
    {
        [JsonProperty("@search.score")]
        public decimal? Score { get; set; }
        [JsonProperty("ULN")]
        public string Uln { get; set; }
        [JsonProperty("Surname")]
        public string Surname { get; set; }
        [JsonProperty("Forename")]
        public string Forename { get; set; }
        public char? Gender { get; set; }
        [JsonProperty("DOB")]
        public DateTime? Dob { get; set; }
    }
}

