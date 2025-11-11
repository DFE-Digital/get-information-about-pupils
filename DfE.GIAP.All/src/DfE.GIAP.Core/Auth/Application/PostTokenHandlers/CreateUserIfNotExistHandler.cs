using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;

namespace DfE.GIAP.Core.Auth.Application.PostTokenHandlers;

public class CreateUserIfNotExistHandler : IPostTokenValidatedHandler
{
    private readonly IUseCaseRequestOnly<CreateUserIfNotExistsRequest> _createUser;
    private readonly IUserContextFactory _userContextFactory;

    public CreateUserIfNotExistHandler(
        IUseCaseRequestOnly<CreateUserIfNotExistsRequest> createUser,
        IUserContextFactory userContextFactory)
    {
        _createUser = createUser;
        _userContextFactory = userContextFactory;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        AuthenticatedUser authUser = _userContextFactory.FromPrincipal(context.Principal);

        // Ensure user exists
        await _createUser.HandleRequestAsync(new CreateUserIfNotExistsRequest(authUser.UserId));
    }
}

