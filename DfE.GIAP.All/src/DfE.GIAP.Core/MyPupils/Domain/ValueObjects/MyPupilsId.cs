using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class MyPupilsId : ValueObject<MyPupilsId>
{
    public MyPupilsId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
