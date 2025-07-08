using DfE.GIAP.Domain.Models.Common;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.User
{
    [ExcludeFromCodeCoverage]
    public class Organisation
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("urn")]
        public string UniqueReferenceNumber { get; set; }

        [JsonProperty("uid")]
        public string UniqueIdentifier { get; set; }

        [JsonProperty("ukprn")]
        public string UKProviderReferenceNumber { get; set; }

        [JsonProperty("statutoryLowAge")]
        public string StatutoryLowAge { get; set; }

        [JsonProperty("statutoryHighAge")]
        public string StatutoryHighAge { get; set; }

        [JsonProperty("category")]
        public DsiCommonResponse Category { get; set; }

        [JsonProperty("type")]
        public DsiCommonResponse EstablishmentType { get; set; }

        [JsonProperty("establishmentNumber")]
        public string EstablishmentNumber { get; set; }

        [JsonProperty("localAuthority")]
        public DsiCommonResponse LocalAuthority { get; set; }

    }
}
