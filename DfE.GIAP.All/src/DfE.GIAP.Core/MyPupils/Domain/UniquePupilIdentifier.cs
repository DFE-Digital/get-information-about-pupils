using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain;
public sealed class UniquePupilIdentifier : ValueObject<UniquePupilIdentifier>
{
    public int Value { get; }

    public UniquePupilIdentifier(int value)
    {
        // Should be a 6 digit positive number
        if (value < 100000 || value > 999999)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "UPN must be a 6-digit positive number.");
        }

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
