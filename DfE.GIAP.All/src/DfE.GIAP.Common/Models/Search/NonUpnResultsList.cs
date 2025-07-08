using DfE.GIAP.Domain.Models.Search;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class NonUpnResultsList : IRbac
    {
        [JsonProperty("@search.score")]
        public decimal Score { get; set; }
        [JsonProperty("UPN")]
        public string LearnerNumber { get; set; }
        public string LearnerNumberId { get; set; }
        public string Surname { get; set; }
        public string Forename { get; set; }
        public char? Gender { get; set; }
        [JsonProperty("DOB")]
        public DateTime? DOB { get; set; }
    }
}
