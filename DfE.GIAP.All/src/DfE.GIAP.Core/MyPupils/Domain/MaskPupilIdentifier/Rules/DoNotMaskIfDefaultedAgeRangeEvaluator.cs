using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierMask.Rules;
internal sealed class DoNotMaskIfDefaultedAgeRangeEvaluator : IEvaluator<MaskPupilIdentifierRequest, bool>
{
    public bool CanEvaluate(MaskPupilIdentifierRequest evaluationRequest) => true;
    public bool Evaluate(MaskPupilIdentifierRequest evaluationRequest) => evaluationRequest.AuthorisationContext.AgeRange.HasNoAgeRange;
}
