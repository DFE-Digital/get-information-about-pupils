using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Entities;

public sealed class Pupil : Entity<PupilId>
{
    private const string MaskedPupilMarker = "*************";
    private readonly UniquePupilNumber _uniquePupilNumber;
    private readonly DateTime? _dateOfBirth;
    private readonly MyPupilsAuthorisationContext _authorisationContext;

    public Pupil(
        PupilId identifier,
        UniquePupilNumber uniquePupilNumber,
        DateTime? dateOfBirth,
        MyPupilsAuthorisationContext authorisationContext)
        : base(identifier)
    {
        _uniquePupilNumber = uniquePupilNumber;
        _dateOfBirth = dateOfBirth;
        _authorisationContext = authorisationContext;
    }

    public bool HasDateOfBirth => _dateOfBirth is not null;
    public string UniquePupilNumber =>
        _authorisationContext.ShouldMaskPupil(this)
            ? MaskedPupilMarker :
                _uniquePupilNumber.Value;

    internal bool TryCalculateAge(out int? calculatedAge)
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
