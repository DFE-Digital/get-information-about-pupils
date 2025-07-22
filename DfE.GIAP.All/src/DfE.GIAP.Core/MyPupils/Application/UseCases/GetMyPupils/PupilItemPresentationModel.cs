using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public record PupilItemPresentationModel
{
    public PupilItemPresentationModel(Pupil pupil)
    {
        Id = pupil.Identifier.Id;
        UniquePupilIdentifier = pupil.UniquePupilNumber;
        DateOfBirth = pupil.DateOfBirth?.ToString() ?? string.Empty;
        FirstName = pupil.FirstName;
        Surname = pupil.Surname;
        Sex = pupil.Sex;
        IsPupilPremium = pupil.IsOfPupilType(Domain.ValueObjects.PupilType.PupilPremium);
        LocalAuthorityCode = pupil.LocalAuthorityCode;
    }

    public string Id { get; } // Uniquely identifies a Pupil in the list
    public string FirstName { get; }
    public string Surname { get; }
    public string UniquePupilIdentifier { get; }
    public string DateOfBirth { get; }
    public string Sex { get; }
    public bool IsPupilPremium { get; }
    public int LocalAuthorityCode { get; }
}
