using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
public record PupilDto
{
    public PupilDto(Pupil pupil)
    {
        Id = pupil.Identifier.Id;
        UniquePupilNumber = pupil.UniquePupilNumber;
        DateOfBirth = pupil.DateOfBirth?.ToString() ?? string.Empty;
        Forename = pupil.Forename;
        Surname = pupil.Surname;
        Sex = pupil.Sex;
        IsPupilPremium = pupil.IsOfPupilType(ValueObjects.PupilType.PupilPremium);
        LocalAuthorityCode = pupil.LocalAuthorityCode;
    }

    public string Id { get; } // Uniquely identifies a Pupil in the list
    public string Forename { get; }
    public string Surname { get; }
    public string UniquePupilNumber { get; }
    public string DateOfBirth { get; }
    public string Sex { get; }
    public bool IsPupilPremium { get; }
    public int LocalAuthorityCode { get;  }
}
