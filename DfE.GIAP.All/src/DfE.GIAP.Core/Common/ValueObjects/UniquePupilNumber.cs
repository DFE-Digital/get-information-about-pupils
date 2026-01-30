using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Common.ValueObjects;
public sealed class UniquePupilNumber : ValueObject<UniquePupilNumber>
{
    public UniquePupilNumber(string value)
    {
        if (!UniquePupilNumberValidator.Validate(value))
        {
            throw new ArgumentException($"Input {value} is not a valid UniquePupilNumber");
        }

        Value = value;
    }

    public string Value { get; }

    public static bool TryCreate(string? input, [NotNullWhen(true)] out UniquePupilNumber? result)
    {
        result = null;
        try
        {
            result = new UniquePupilNumber(input!);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
