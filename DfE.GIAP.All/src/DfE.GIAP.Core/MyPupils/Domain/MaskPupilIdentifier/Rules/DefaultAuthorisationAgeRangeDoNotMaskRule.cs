using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
using DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierMask.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierMask.Rules;
internal sealed class DefaultAuthorisationAgeRangeDoNotMaskRule : IMaskPupilIdentifierRule
{
    public bool ShouldMask(MyPupilsAuthorisationContext authorisationContext, Pupil pupil) => authorisationContext.AgeRange.HasNoAgeRange;
}
