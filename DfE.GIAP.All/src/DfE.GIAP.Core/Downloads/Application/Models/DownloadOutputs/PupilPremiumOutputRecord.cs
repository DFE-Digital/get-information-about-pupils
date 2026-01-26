namespace DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

public class PupilPremiumOutputRecord
{
    public string? UniquePupilNumber { get; set; }
    public string? Surname { get; set; }
    public string? Forename { get; set; }
    public string? Sex { get; set; }
    public string? DOB { get; set; }
    public string? NCYear { get; set; }
    public int? DeprivationPupilPremium { get; set; }
    public int? ServiceChildPremium { get; set; }
    public int? AdoptedfromCarePremium { get; set; }
    public int? LookedAfterPremium { get; set; }
    public string? PupilPremiumFTE { get; set; }
    public string? PupilPremiumCashAmount { get; set; }
    public string? PupilPremiumFYStartDate { get; set; }
    public string? PupilPremiumFYEndDate { get; set; }
    public string? LastFSM { get; set; }
}
