using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.SecurityReports
{
    [ExcludeFromCodeCoverage]
    public class Establishment
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("URN")]
        public string URN { get; set; }

        public string Description => $"{Name}: {URN}";
    }
}
