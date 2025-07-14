using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.PupilIdentifierEncoder;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.ValueObjects;
public record MyPupilIdentifier
{
    public MyPupilIdentifier(
        UniquePupilIdentifier original,
        ShouldMaskPupilIdentifier response,
        IPupilIdentifierEncoder encoder)
    {
        Value = response.ShouldMask ?
            encoder.Encode(original)
                : original.ToString()!;
    }

    public string Value { get; init; }
}
