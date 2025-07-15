using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain;
public sealed class Pupil : Entity<UniquePupilIdentifier>
{
    private readonly DateTime? _dateOfBirth;

    public Pupil(
        UniquePupilIdentifier identifier,
        DateTime? dateOfBirth) : base(identifier)
    {
        _dateOfBirth = dateOfBirth;

    }

    public bool HasDateOfBirth => _dateOfBirth is not null;

    public bool TryCalculateAge(out int? calculatedAge)
    {
        calculatedAge = null;

        if (!HasDateOfBirth)
        {
            return false;
        }

        DateTime today = DateTime.Today;
        int pupilYearOfBirth = _dateOfBirth!.Value.Year;

        calculatedAge = today.Year - pupilYearOfBirth;
        // If the pupil has not had their birthday this year, then we need to decrement their age by 1.
        if (pupilYearOfBirth > today.AddYears(-calculatedAge.Value).Year)
        {
            calculatedAge--;
        }
        return true;
    }
}

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
