namespace DfE.GIAP.Core.Downloads.Application.Models;

public class FurtherEducationPupil
{
    public string? UniqueLearnerNumber { get; set; }
    public string? Forename { get; set; }
    public string? Surname { get; set; }
    public string? Gender { get; set; }
    public DateTime DOB { get; set; }
    public string? ConcatenatedName { get; set; }
    public List<PupilPremiumEntry>? PupilPremium { get; set; }
    public List<SpecialEducationalNeedsEntry>? specialEducationalNeeds { get; set; }

    public bool HasPupilPremiumData => PupilPremium?.Any() ?? false;
    public bool HasSpecialEducationalNeedsData => specialEducationalNeeds?.Any() ?? false;
}
