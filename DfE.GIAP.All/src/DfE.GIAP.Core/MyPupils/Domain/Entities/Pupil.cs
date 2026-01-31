using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Entities;
public sealed class Pupil : Entity<UniquePupilNumber>
{
    private readonly PupilType _pupilType;
    private readonly PupilName _name;
    private readonly DateOfBirth? _dateOfBirth;
    private readonly Sex? _sex;

    public Pupil(
        UniquePupilNumber identifier,
        PupilType pupilType,
        PupilName name,
        DateTime? dateOfBirth,
        Sex? sex,
        LocalAuthorityCode? localAuthorityCode)
        : base(identifier)
    {
        ArgumentNullException.ThrowIfNull(name);
        _pupilType = pupilType;
        _name = name;
        _dateOfBirth = dateOfBirth is null ? null : new DateOfBirth(dateOfBirth.Value);
        _sex = sex;
        LocalAuthorityCode = localAuthorityCode?.Code ?? null;
    }

    public string Forename => _name.Forename;
    public string Surname => _name.Surname;
    public DateTime? TryParseDateOfBirth()
    {
        return _dateOfBirth is null ?
            null :
                Convert.ToDateTime(_dateOfBirth);
    }

    public int? LocalAuthorityCode { get; }
    public string Sex => _sex?.ToString() ?? string.Empty;
    public bool IsOfPupilType(PupilType pupilType) => _pupilType.Equals(pupilType);
    internal bool TryCalculateAge(out int? calculatedAge)
    {
        calculatedAge = null;

        if (_dateOfBirth is null)
        {
            return false;
        }

        calculatedAge = _dateOfBirth!.Age;
        return true;
    }
}
