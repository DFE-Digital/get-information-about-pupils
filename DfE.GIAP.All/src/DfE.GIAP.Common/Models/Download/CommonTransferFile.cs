using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Download
{
    [ExcludeFromCodeCoverage]
    public class CommonTransferFile
    {
        [JsonProperty("UPNs")]
        public string[] UPNs { get; set; }
        public string[] SortOrder { get; set; }
        public bool IsEstablishment { get; set; }
        public string LocalAuthorityNumber { get; set; }
        public string EstablishmentNumber { get; set; }
    }
}