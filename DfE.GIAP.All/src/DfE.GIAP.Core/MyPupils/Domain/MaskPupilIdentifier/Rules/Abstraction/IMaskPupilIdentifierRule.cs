using DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierMask.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
public interface IMaskPupilIdentifierRule
{
    public bool ShouldMask(MyPupilsAuthorisationContext authorisationContext, Pupil pupil);
}
