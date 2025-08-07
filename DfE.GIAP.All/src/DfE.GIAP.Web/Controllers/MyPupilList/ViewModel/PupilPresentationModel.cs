using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;

public sealed class PupilPresentatationModel
{
    public PupilPresentatationModel(PupilDto dto)
    {
        UniquePupilNumber = dto.UniquePupilNumber;
        Forename = dto.Forename;
        Surname = dto.Surname;
        DateOfBirth = dto.DateOfBirth;
        PupilPremiumLabel = dto.IsPupilPremium ? "Yes" : "No";
        Sex = dto.Sex;
        LocalAuthorityCode = dto.LocalAuthorityCode.ToString();
    }
    public string UniquePupilNumber { get; }
    public string Forename { get; }
    public string Surname { get; }
    public string DateOfBirth { get; }
    public string PupilPremiumLabel { get; }
    public string Sex { get; }
    public string LocalAuthorityCode { get; }
}
