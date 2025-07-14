using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules;
internal sealed class MaskIfDateOfBirthDoesNotExistEvaluator : IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>
{
    public bool CanEvaluate(MaskPupilIdentifierRequest evaluationRequest) => !evaluationRequest.Pupil.HasDateOfBirth;
    public ShouldMaskPupilIdentifier Evaluate(MaskPupilIdentifierRequest evaluationRequest) => ShouldMaskPupilIdentifier.Mask;
}

