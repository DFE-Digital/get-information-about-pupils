using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules.Abstraction;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils;
public record MyPupil
{
    public required MyPupilIdentifier Id { get; init; }
}
