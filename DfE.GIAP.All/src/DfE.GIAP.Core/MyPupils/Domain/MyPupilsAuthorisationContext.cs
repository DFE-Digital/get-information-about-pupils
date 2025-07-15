using DfE.GIAP.Core.Common.Domain.Pupil;
using DfE.GIAP.Core.Common.Domain.User;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain;
public record MyPupilsAuthorisationContext
{
    private readonly UserIdentifier _userId;
    private readonly AgeRange _authorisedAgeRange;

    public MyPupilsAuthorisationContext(UserIdentifier userId, AgeRange AgeRange)
    {
        ArgumentNullException.ThrowIfNull(userId);
        _userId = userId;
        _authorisedAgeRange = AgeRange;
    }

    private bool IsAgeInRange(int age)
        => age >= _authorisedAgeRange.Low &&
            age <= _authorisedAgeRange.High;

    public bool ShouldMaskPupil(Pupil pupil)
    {
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
