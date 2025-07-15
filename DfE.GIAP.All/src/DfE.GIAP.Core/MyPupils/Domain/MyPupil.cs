using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain;
public record MyPupil
{
    public required MyPupilIdentifier Id { get; init; }
}
