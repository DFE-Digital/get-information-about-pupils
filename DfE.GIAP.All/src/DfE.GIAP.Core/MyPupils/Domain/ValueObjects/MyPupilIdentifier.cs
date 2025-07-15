using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.PupilIdentifierEncoder;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public record MyPupilIdentifier
{
    public MyPupilIdentifier(
        UniquePupilIdentifier id,
        ShouldMaskPupilIdentifier mask,
        IPupilIdentifierEncoder encoder)
    {
        Value = mask.ShouldMask ?
            encoder.Encode(id)
                : id.ToString()!;
    }

    public string Value { get; init; }
}
