namespace DfE.GIAP.Core.MyPupils.Domain;
public sealed class Pupil
{
    private readonly int _upn;
    private readonly DateTime? _dateOfBirth;

    // TODO PupilIdentifier
    public Pupil(int URN, DateTime? dateOfBirth)
    {
        _upn = URN;
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
