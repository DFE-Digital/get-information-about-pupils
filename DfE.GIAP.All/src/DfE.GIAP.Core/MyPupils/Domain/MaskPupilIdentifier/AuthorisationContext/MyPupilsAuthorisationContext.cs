using DfE.GIAP.Core.Common.Domain.User;

namespace DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.AuthorisationContext;
public record MyPupilsAuthorisationContext(UserId userId, AgeRange AgeRange);
