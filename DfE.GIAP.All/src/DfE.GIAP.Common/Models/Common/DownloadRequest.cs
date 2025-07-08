using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Common
{
    [ExcludeFromCodeCoverage]
    public class DownloadRequest
    {
        [JsonProperty("UPNs")]
        public string[] UPNs { get; set; }

        public string[] SortOrder { get; set; }

        [JsonProperty("DataTypes")]
        public string[] DataTypes { get; set; }

        public bool ConfirmationGiven { get; set; }
        public UserOrganisation UserOrganisation { get; set; }
        public string FileType { get; set; }
        public bool CheckOnly { get; set; }
    }
}