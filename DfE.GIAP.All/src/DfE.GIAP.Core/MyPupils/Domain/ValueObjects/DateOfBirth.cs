using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class DateOfBirth : ValueObject<DateOfBirth>
{
    // TODO leap years
    // TODO timezones

    private readonly DateTime _value;

    public DateOfBirth(DateTime value)
    {
        if (value > DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Date of birth cannot be in the future.");
        }
        _value = value;
    }

    public int Age => CalculateAge();
    private int Year => _value.Year;
    private int Day => _value.Day;
    private int Month => _value.Month;

    private int CalculateAge()
    {
        DateTime today = DateTime.UtcNow; // Assumes UTC so inaccurate with BST for 1 hour?

        bool hasOccuredThisMonth =
            (today.Month > Month) ||
                (today.Month == Month && today.Day >= Day);

        return hasOccuredThisMonth ?
            today.Year - Year :
                today.Year - Year - 1;
    }


    public override string ToString() => _value.ToString("yyyy-MM-dd") ?? string.Empty;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return _value!;
    }

    public static implicit operator DateTime?(DateOfBirth dob) => dob._value;
}
