using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierEncoder;
using DfE.GIAP.Core.Pupil.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class MyPupilIdentifier : ValueObject<MyPupilIdentifier>
{
    public MyPupilIdentifier(
        UniquePupilIdentifier id,
        IPupilIdentifierEncoder encoder)
    {
        Value = encoder.Encode(id);
    }

    public string Value { get; init; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
