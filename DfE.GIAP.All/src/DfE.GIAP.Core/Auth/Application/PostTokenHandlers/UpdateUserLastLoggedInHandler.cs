using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;

namespace DfE.GIAP.Core.Auth.Application.PostTokenHandlers;

public class UpdateUserLastLoggedInHandler : IPostTokenValidatedHandler
{
    private readonly IUseCaseRequestOnly<UpdateLastLoggedInRequest> _updateLastLogin;
    private readonly IUserContextFactory _userContextFactory;

    public UpdateUserLastLoggedInHandler(
        IUseCaseRequestOnly<UpdateLastLoggedInRequest> updateLastLogin,
        IUserContextFactory userContextFactory)
    {
        _updateLastLogin = updateLastLogin;
        _userContextFactory = userContextFactory;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        AuthenticatedUser authUser = _userContextFactory.FromPrincipal(context.Principal);

        // Update last login
        await _updateLastLogin.HandleRequestAsync(
            new UpdateLastLoggedInRequest(authUser.UserId, DateTime.UtcNow));
    }
}

