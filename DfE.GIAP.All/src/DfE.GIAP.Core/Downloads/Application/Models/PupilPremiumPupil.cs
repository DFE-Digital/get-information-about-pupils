using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Application.Models;

public class PupilPremiumPupil
{
    public string? UniquePupilNumber { get; set; }
    public string? UniqueReferenceNumber { get; set; }
    public string? Forename { get; set; }
    public string? Surname { get; set; }
    public string? Sex { get; set; }
    public DateTime DOB { get; set; }
    public string? ConcatenatedName { get; set; }
    public List<PupilPremiumEntry> PupilPremium { get; set; } = new();
}
