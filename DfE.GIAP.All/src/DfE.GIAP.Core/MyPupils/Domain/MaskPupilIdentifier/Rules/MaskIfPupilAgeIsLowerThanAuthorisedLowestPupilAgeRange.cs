using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierMask.Rules;
internal sealed class MaskIfPupilAgeIsLowerThanAuthorisedLowestPupilAgeRange : IEvaluator<MaskPupilIdentifierRequest, bool>
{
    public bool CanEvaluate(MaskPupilIdentifierRequest evaluationRequest) => true;
    public bool Evaluate(MaskPupilIdentifierRequest evaluationRequest)
    {
        if (!evaluationRequest.Pupil.TryCalculateAge(out int? calculatedAge) || calculatedAge is null)
        {
            return false;
        }
        return calculatedAge < evaluationRequest.AuthorisationContext.AgeRange.Low;
    }
}
