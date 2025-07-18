using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Entities;

public sealed class Pupil : Entity<PupilId>
{
    private const string MaskedPupilMarker = "*************";
    private readonly UniquePupilNumber _uniquePupilNumber;
    private readonly DateOfBirth? _dateOfBirth;
    private readonly PupilAuthorisationContext _authorisationContext;

    public Pupil(
        PupilId identifier,
        UniquePupilNumber uniquePupilNumber,
        DateTime? dateOfBirth,
        PupilAuthorisationContext authorisationContext)
        : base(identifier)
    {
        _uniquePupilNumber = uniquePupilNumber;
        _dateOfBirth = dateOfBirth is null ? null : new DateOfBirth(dateOfBirth.Value);
        _authorisationContext = authorisationContext;
    }

    public bool HasDateOfBirth => _dateOfBirth is not null;
    public string UniquePupilNumber =>
        _authorisationContext.ShouldMaskPupil(this)
            ? MaskedPupilMarker :
                _uniquePupilNumber.Value;

    public string? DateOfBirth => _dateOfBirth?.ToString();

    internal bool TryCalculateAge(out int? calculatedAge)
    {
        calculatedAge = null;

        if (!HasDateOfBirth)
        {
            return false;
        }

        calculatedAge = _dateOfBirth!.Age;
        return true;

    }
    // TODO expose fields that extract and format out of Pupil
}
