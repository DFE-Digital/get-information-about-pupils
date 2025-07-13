using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
using DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierMask.Rules.Abstraction;

namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules;
internal class MaskIfDateOfBirthDoesNotExistRule : IMaskPupilIdentifierRule
{
    public bool ShouldMask(MyPupilsAuthorisationContext authorisationContext, Pupil pupil) => !pupil.HasDateOfBirth;
}

