using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
public record MaskPupilIdentifierRequest(MyPupilsAuthorisationContext AuthorisationContext, Pupil Pupil);
