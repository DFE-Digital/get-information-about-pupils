using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules.Abstraction;
public record MaskPupilIdentifierRequest(MyPupilsAuthorisationContext AuthorisationContext, Pupil Pupil);
