using DfE.GIAP.Core.MyPupils.Domain.Aggregate;

namespace DfE.GIAP.Web.Controllers.MyPupilList;

public sealed class PupilPresentatationModel
{
    public PupilPresentatationModel(PupilDto dto)
    {
        Id = dto.Id;
        UniquePupilNumber = dto.UniquePupilNumber;
        Forename = dto.Forename;
        Surname = dto.Surname;
        DateOfBirth = dto.DateOfBirth;
        PupilPremiumLabel = dto.IsPupilPremium ? "Yes" : "No";
        Sex = dto.Sex;
        LocalAuthorityCode = dto.LocalAuthorityCode.ToString();
    }

    public string Id { get; }
    public string UniquePupilNumber { get; }
    public string Forename { get; }
    public string Surname { get; }
    public string DateOfBirth { get; }
    public string PupilPremiumLabel { get; }
    public string Sex { get; }
    public string LocalAuthorityCode { get; }
}
