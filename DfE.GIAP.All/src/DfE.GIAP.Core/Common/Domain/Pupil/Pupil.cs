namespace DfE.GIAP.Core.Common.Domain.Pupil;
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


