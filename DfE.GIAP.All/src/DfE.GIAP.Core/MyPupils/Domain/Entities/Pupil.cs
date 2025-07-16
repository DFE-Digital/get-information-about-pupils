using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Entities;

public sealed class Pupil : Entity<PupilId>
{
    private readonly DateTime? _dateOfBirth;

    public Pupil(
        PupilId identifier,
        DateTime? dateOfBirth)
        : base(identifier)
    {
        _dateOfBirth = dateOfBirth;
    }

    public bool HasDateOfBirth => _dateOfBirth is not null;
    public string UniquePupilNumber => Identifier.Upn;
    public bool TryCalculateAge(out int? calculatedAge)
    {
        calculatedAge = null;

        if (!HasDateOfBirth)
        {
            return false;
        }
            
        DateTime today = DateTime.Today;
        DateTime pupilDob = _dateOfBirth!.Value;

        int age = today.Year - pupilDob.Year;

        // If birthday hasn't occurred yet this year, subtract 1
        if (today < pupilDob.AddYears(age))
        {
            age--;
        }

        calculatedAge = age;
        return true;

    }
    // TODO expose fields that extract and format out of Pupil
}
