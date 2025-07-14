using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules;
internal sealed class MaskIfAuthorisationContextHasDefaultedAgeRangeEvaluator : IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>
{
    public bool CanEvaluate(MaskPupilIdentifierRequest evaluationRequest) => evaluationRequest.AuthorisationContext.AgeRange.HasNoAgeRange;
    public ShouldMaskPupilIdentifier Evaluate(MaskPupilIdentifierRequest evaluationRequest) => ShouldMaskPupilIdentifier.Mask;
}
