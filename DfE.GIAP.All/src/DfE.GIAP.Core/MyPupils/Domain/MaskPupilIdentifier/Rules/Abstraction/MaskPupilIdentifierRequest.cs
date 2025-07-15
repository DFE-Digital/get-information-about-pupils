using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.AuthorisationContext;

namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
public record MaskPupilIdentifierRequest(MyPupilsAuthorisationContext AuthorisationContext, Pupil Pupil);
