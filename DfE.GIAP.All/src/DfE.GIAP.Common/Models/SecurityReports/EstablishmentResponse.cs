using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.SecurityReports
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentResponse
    {
        [JsonProperty("Establishments")]
        public IEnumerable<Establishment> Establishments { get; set; }
    }
}
