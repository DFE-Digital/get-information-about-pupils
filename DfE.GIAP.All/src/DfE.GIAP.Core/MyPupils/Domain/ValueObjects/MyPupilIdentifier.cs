using DfE.GIAP.Core.Common.Domain.Contracts;
using DfE.GIAP.Core.Common.Domain.Pupil;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.PupilIdentifierEncoder;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class MyPupilIdentifier : ValueObject<MyPupilIdentifier>
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

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
