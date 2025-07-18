using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public record PupilItemPresentationModel
{
    public PupilItemPresentationModel(
        PupilId pupilId,
        string upn,
        DateOfBirth? dateOfBirth)
    {
        Id = pupilId.Id;
        UniquePupilIdentifier = upn;
        DateOfBirth = dateOfBirth?.ToString() ?? string.Empty;
    }

    public string Id { get; } // Uniquely identifies a Pupil in the list
    public string UniquePupilIdentifier { get; }
    public string DateOfBirth { get; }
}
