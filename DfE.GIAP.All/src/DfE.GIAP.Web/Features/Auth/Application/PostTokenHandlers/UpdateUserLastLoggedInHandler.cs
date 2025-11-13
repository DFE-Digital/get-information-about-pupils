using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure;

namespace DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;

public class UpdateUserLastLoggedInHandler : IPostTokenValidatedHandler
{
    private readonly IUseCaseRequestOnly<UpdateLastLoggedInRequest> _updateLastLoggedInUseCase;

    public UpdateUserLastLoggedInHandler(IUseCaseRequestOnly<UpdateLastLoggedInRequest> updateLastLoggedInUseCase)
    {
        ArgumentNullException.ThrowIfNull(updateLastLoggedInUseCase);
        _updateLastLoggedInUseCase = updateLastLoggedInUseCase;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        AuthenticatedUser authUser = UserContextFactory.FromPrincipal(context.Principal);

        await _updateLastLoggedInUseCase.HandleRequestAsync(
            new UpdateLastLoggedInRequest(authUser.UserId, DateTime.UtcNow));
    }
}

