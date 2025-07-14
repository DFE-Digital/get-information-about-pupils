using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
using DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierMask.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules;
internal class PupilAgeIsHigherThanAuthorisedHighestPupilAgeMaskRule : IMaskPupilIdentifierRule
{
    public bool ShouldMask(MyPupilsAuthorisationContext authorisationContext, Pupil pupil)
    {
        if (!pupil.TryCalculateAge(out int? calculatedAge) || calculatedAge is null)
        {
            return false;
        }
        return calculatedAge > authorisationContext.AgeRange.High;
    }
}
