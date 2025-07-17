using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Authorisation;
public record MyPupilsAuthorisationContext
{
    private readonly AgeRange _authorisedAgeRange;
    private readonly UserRole _userRole;

    public MyPupilsAuthorisationContext(
        AgeRange AgeRange,
        UserRole role)
    {
        _authorisedAgeRange = AgeRange;
        _userRole = role;
    }

    private bool IsAgeInRange(int age)
        => age >= _authorisedAgeRange.Low &&
            age <= _authorisedAgeRange.High;

    public bool ShouldMaskPupil(Pupil pupil)
    {
        if (_userRole.IsAdministrator)
        {
            return false;
        }

        if (_authorisedAgeRange.IsDefaultedRange) // RBAC rules don't apply and should not be masked
        {
            return false;
        }

        if (!pupil.HasDateOfBirth)
        {
            return true;
        }

        if (!pupil.TryCalculateAge(out int? age) || age is null)
        {
            return true;
        }

        if (!IsAgeInRange(age.Value))
        {
            return true;
        }

        return false;
    }
}

