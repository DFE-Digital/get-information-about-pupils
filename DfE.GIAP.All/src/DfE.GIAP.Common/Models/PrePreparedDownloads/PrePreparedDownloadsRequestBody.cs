using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.PrePreparedDownloads
{
    [ExcludeFromCodeCoverage]
    public class PrePreparedDownloadsRequestBody
    {
        [JsonProperty("IsOrganisationLocalAuthority")]
        public string IsOrganisationLocalAuthority { get; set; }
        [JsonProperty("IsOrganisationMultiAcademyTrust")]
        public string IsOrganisationMultiAcademyTrust { get; set; }
        [JsonProperty("IsOrganisationEstablishment")]
        public string IsOrganisationEstablishment { get; set; }
        [JsonProperty("IsOrganisationSingleAcademyTrust")]
        public string IsOrganisationSingleAcademyTrust { get; set; }
        [JsonProperty("EstablishmentNumber")] 
        public string EstablishmentNumber  { get; set; }
        [JsonProperty("UniqueIdentifier")]
        public string UniqueIdentifier  { get; set; }
        [JsonProperty("LocalAuthorityNumber")]
        public string LocalAuthorityNumber  { get; set; }
        [JsonProperty("UniqueReferenceNumber")]
        public string UniqueReferenceNumber { get; set; }
    }
}
