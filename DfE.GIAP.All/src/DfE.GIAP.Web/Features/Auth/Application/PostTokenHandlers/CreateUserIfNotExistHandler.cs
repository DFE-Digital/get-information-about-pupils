using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Web.Features.Auth.Application.Models;

namespace DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;

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

        await _createUser.HandleRequestAsync(new CreateUserIfNotExistsRequest(authUser.UserId));
    }
}

