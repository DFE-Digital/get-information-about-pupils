using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
public record PupilDto
{
    public required string Id { get; init; } // Uniquely identifies a Pupil in the list
    public required string Forename { get; init; }
    public required string Surname { get; init; }
    public required string UniquePupilNumber { get; init; }
    public required string DateOfBirth { get; init; }
    public required string Sex { get; init; }
    public required bool IsPupilPremium { get; init; }
    public required int LocalAuthorityCode { get; init;  }
}
