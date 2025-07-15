using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules;
internal sealed class MaskIfPupilAgeIsHigherThanAuthorisedHighestPupilAgeEvaluator : IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>
{
    public bool CanEvaluate(MaskPupilIdentifierRequest evaluationRequest)
    {
        if (!evaluationRequest.Pupil.TryCalculateAge(out int? calculatedAge) || calculatedAge is null)
        {
            return false;
        }
        return calculatedAge > evaluationRequest.AuthorisationContext.AgeRange.High;
    }

    public ShouldMaskPupilIdentifier Evaluate(MaskPupilIdentifierRequest evaluationRequest) => ShouldMaskPupilIdentifier.Mask;
}
