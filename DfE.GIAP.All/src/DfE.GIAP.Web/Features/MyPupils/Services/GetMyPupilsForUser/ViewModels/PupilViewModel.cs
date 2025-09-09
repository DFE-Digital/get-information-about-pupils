namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;

public record PupilViewModel
{
    public string UniquePupilNumber { get; init; }
    public string Forename { get; init; }
    public string Surname { get; init; }
    public string DateOfBirth { get; init; }
    public string PupilPremiumLabel { get; init; }
    public string Sex { get; init; }
    public string LocalAuthorityCode { get; init; }
    public bool IsSelected { get; init; }
}
