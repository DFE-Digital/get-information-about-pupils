using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules;
internal sealed class MaskIfDateOfBirthDoesNotExistEvaluator : IEvaluator<MaskPupilIdentifierRequest, bool>
{
    public bool CanEvaluate(MaskPupilIdentifierRequest evaluationRequest) => true;
    public bool Evaluate(MaskPupilIdentifierRequest evaluationRequest) => !evaluationRequest.Pupil.HasDateOfBirth;
}

