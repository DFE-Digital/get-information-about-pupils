using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Entities;

public sealed class Pupil : Entity<PupilId>
{
    private const string MaskedPupilMarker = "*************";
    private readonly PupilType _pupilType;
    private readonly PupilName _name;
    private readonly UniquePupilNumber _uniquePupilNumber;
    private readonly DateOfBirth? _dateOfBirth;
    private readonly PupilAuthorisationContext _authorisationContext;

    public Pupil(
        PupilId identifier,
        PupilType pupilType,
        PupilName name,
        UniquePupilNumber uniquePupilNumber,
        DateTime? dateOfBirth,
        PupilAuthorisationContext authorisationContext)
        : base(identifier)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(uniquePupilNumber);
        ArgumentNullException.ThrowIfNull(authorisationContext);
        _pupilType = pupilType;
        _name = name;
        _uniquePupilNumber = uniquePupilNumber;
        _dateOfBirth = dateOfBirth is null ? null : new DateOfBirth(dateOfBirth.Value);
        _authorisationContext = authorisationContext;
    }

    public bool HasDateOfBirth => _dateOfBirth is not null;
    public string UniquePupilNumber =>
        _authorisationContext.ShouldMaskPupil(this)
            ? MaskedPupilMarker :
                _uniquePupilNumber.Value;

    public DateOfBirth? DateOfBirth => _dateOfBirth;
    public string FirstName => _name.FirstName;
    public string Surname => _name.Surname;
    public bool IsOfPupilType(PupilType pupilType) => _pupilType.Equals(pupilType);
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
}
