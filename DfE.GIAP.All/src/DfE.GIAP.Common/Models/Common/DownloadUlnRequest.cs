using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Common
{
    [ExcludeFromCodeCoverage]
    public class DownloadUlnRequest
    {
        [JsonProperty("ULNs")]
        public string[] ULNs { get; set; }
        [JsonProperty("DataTypes")]
        public string[] DataTypes { get; set; }
        public bool ConfirmationGiven { get; set; }
        public bool CheckOnly { get; set; }
    }
}
