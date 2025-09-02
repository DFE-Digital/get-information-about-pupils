using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;

public record UpdateLastLoggedInRequest(string UserId, DateTime LastLoggedIn) : IUseCaseRequest;
