using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.SecurityReports
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentRequest
    {
        [JsonProperty("CODE")]
        public string Code { get; set; }

        [JsonProperty("TYPE")]
        public string Type { get; set; }
    }
}
