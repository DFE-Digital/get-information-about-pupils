using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules;
internal sealed class MaskIfPupilAgeIsLowerThanAuthorisedLowestPupilAgeRange : IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>
{
    public bool CanEvaluate(MaskPupilIdentifierRequest evaluationRequest)
    {
        if (!evaluationRequest.Pupil.TryCalculateAge(out int? calculatedAge) || calculatedAge is null)
        {
            return false;
        }
        return calculatedAge < evaluationRequest.AuthorisationContext.AgeRange.Low;
    }
    public ShouldMaskPupilIdentifier Evaluate(MaskPupilIdentifierRequest evaluationRequest) => ShouldMaskPupilIdentifier.Mask;
}
