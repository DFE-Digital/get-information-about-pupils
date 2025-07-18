using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public record PupilItemPresentationModel
{
    public PupilItemPresentationModel(
        PupilId pupilId,
        string upn,
        string? dateOfBirth) // TODO consider ValueObject?
    {
        Id = pupilId.Id;
        UniquePupilIdentifier = upn;
        DateOfBirth = dateOfBirth;
    }

    public string Id { get; } // Uniquely identifies a Pupil in the list
    public string UniquePupilIdentifier { get; }
    public string? DateOfBirth { get; }
}
