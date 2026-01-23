using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects.Entries;

public class PupilPremiumEntryDto
{
    [JsonProperty("UPN")]
    public string? UniquePupilNumber { get; set; }

    [JsonProperty("Surname")]
    public string? Surname { get; set; }

    [JsonProperty("Forename")]
    public string? Forename { get; set; }

    [JsonProperty("Sex")]
    public string? Sex { get; set; }

    [JsonProperty("DOB")]
    public string? DateOfBirth { get; set; }

    [JsonProperty("NCYear")]
    public string? NCYear { get; set; }

    [JsonProperty("DeprivationPupilPremium")]
    public int DeprivationPupilPremium { get; set; }

    [JsonProperty("ServiceChildPremium")]
    public int ServiceChildPremium { get; set; }

    [JsonProperty("AdoptedfromCarePremium")]
    public int AdoptedfromCarePremium { get; set; }

    [JsonProperty("LookedAfterPremium")]
    public int LookedAfterPremium { get; set; }

    [JsonProperty("PupilPremiumFTE")]
    public string? PupilPremiumFTE { get; set; }

    [JsonProperty("PupilPremiumCashAmount")]
    public string? PupilPremiumCashAmount { get; set; }

    [JsonProperty("PupilPremiumFYStartDate")]
    public string? PupilPremiumFYStartDate { get; set; }

    [JsonProperty("PupilPremiumFYEndDate")]
    public string? PupilPremiumFYEndDate { get; set; }

    [JsonProperty("LastFSM")]
    public string? LastFSM { get; set; }

    [JsonProperty("MODSERVICE")]
    public string? MODSERVICE { get; set; }

    [JsonProperty("CENSUSSERVICEEVER6")]
    public string? CENSUSSERVICEEVER6 { get; set; }
}
