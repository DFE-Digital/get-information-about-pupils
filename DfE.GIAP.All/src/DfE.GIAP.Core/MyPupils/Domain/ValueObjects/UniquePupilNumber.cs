using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class UniquePupilNumber : ValueObject<UniquePupilNumber>
{
    public UniquePupilNumber(string value)
    {
        // TODO what is the shape of a UPN 1/2 chars at the start, some amount of numbers?
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public static bool TryCreate(string? input, [NotNullWhen(true)] out UniquePupilNumber? result)
    {
        result = null;

        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            result = new UniquePupilNumber(input);
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
