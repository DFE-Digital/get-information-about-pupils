using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DfE.GIAP.Domain.Models.SecurityReports
{
    [ExcludeFromCodeCoverage]
    public class AcademyTrust
    {
        [JsonProperty("GroupName")]
        public string Name { get; set; }
        [JsonProperty("Id")]
        public string Code { get; set; }
        public string Description => $"{Name}: {Code}";

        [JsonProperty("Establishments")]
        public IEnumerable<Establishment> Establishments { get; set; }

        [JsonProperty("DOCTYPE")]
        public string DocType { get; set; }
    }
}
