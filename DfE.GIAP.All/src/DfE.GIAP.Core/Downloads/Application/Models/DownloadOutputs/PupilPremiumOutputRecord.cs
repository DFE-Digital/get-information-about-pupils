using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

public class PupilPremiumOutputRecord
{
    public string? UPN { get; set; }
    public string? Surname { get; set; }
    public string? Forename { get; set; }
    public string? Sex { get; set; }
    public string? DOB { get; set; }
    public string? NCYear { get; set; }
    [Display(Name = "Deprivation Pupil Premium")]
    public int? DeprivationPupilPremium { get; set; }
    [Display(Name = "Service Child Premium")]
    public int? ServiceChildPremium { get; set; }
    [Display(Name = "Adopted from Care Premium")]
    public int? AdoptedfromCarePremium { get; set; }
    [Display(Name = "Looked After Premium")]
    public int? LookedAfterPremium { get; set; }
    public string? PupilPremiumFTE { get; set; }
    public string? PupilPremiumCashAmount { get; set; }
    public string? PupilPremiumFYStartDate { get; set; }
    public string? PupilPremiumFYEndDate { get; set; }
    public string? LastFSM { get; set; }
}
