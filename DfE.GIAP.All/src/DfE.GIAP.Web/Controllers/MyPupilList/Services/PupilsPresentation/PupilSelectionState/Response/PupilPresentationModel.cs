namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Response;

public sealed class PupilPresentatationViewModel
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
