using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class PupilDetail
    {
        [JsonProperty("Forename")]
        public string FirstName { get; set; }
        [JsonProperty("Middlenames")]
        public string MiddleName { get; set; }
        [JsonProperty("Surname")]
        public string Surname { get; set; }
        [JsonProperty("UPN")]
        public string UniquePupilNumber { get; set; }
        [JsonProperty("DOB")]
        public string DateOfBirth { get; set; }
        [JsonProperty("Gender")]
        public char? Gender { get; set; }
    }
}