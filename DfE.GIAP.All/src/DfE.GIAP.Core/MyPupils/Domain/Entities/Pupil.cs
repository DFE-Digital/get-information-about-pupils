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
    private readonly Sex? _sex;
    private readonly PupilAuthorisationContext _authorisationContext;

    public Pupil(
        PupilId identifier,
        PupilType pupilType,
        PupilName name,
        UniquePupilNumber uniquePupilNumber,
        DateTime? dateOfBirth,
        Sex? sex,
        LocalAuthorityCode localAuthorityCode,
        PupilAuthorisationContext authorisationContext)
        : base(identifier)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(uniquePupilNumber);
        ArgumentNullException.ThrowIfNull(authorisationContext);
        _pupilType = pupilType;
        _name = name;
        _uniquePupilNumber = uniquePupilNumber;
        _sex = sex;
        _dateOfBirth = dateOfBirth is null ? null : new DateOfBirth(dateOfBirth.Value);
        LocalAuthorityCode = localAuthorityCode.Code;
        _authorisationContext = authorisationContext;
    }

    public string UniquePupilNumber => _authorisationContext.ShouldMaskPupil(this) ? MaskedPupilMarker : _uniquePupilNumber.Value;
    public string Forename => _name.FirstName;
    public string Surname => _name.Surname;
    public bool HasDateOfBirth => _dateOfBirth is not null;
    public string DateOfBirth => _dateOfBirth?.ToString() ?? string.Empty;
    public int LocalAuthorityCode { get; }
    public string Sex => _sex?.ToString() ?? string.Empty;
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
