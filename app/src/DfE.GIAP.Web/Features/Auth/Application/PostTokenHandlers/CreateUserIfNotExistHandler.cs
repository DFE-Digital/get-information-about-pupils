using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure;

namespace DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;

public class CreateUserIfNotExistHandler : IPostTokenValidatedHandler
{
    private readonly IUseCaseRequestOnly<CreateUserIfNotExistsRequest> _createUserIfNotExistsUseCase;

    public CreateUserIfNotExistHandler(IUseCaseRequestOnly<CreateUserIfNotExistsRequest> createUserIfNotExistsUseCase)
    {
        ArgumentNullException.ThrowIfNull(createUserIfNotExistsUseCase);
        _createUserIfNotExistsUseCase = createUserIfNotExistsUseCase;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        AuthenticatedUser authUser = UserContextFactory.FromPrincipal(context.Principal);

        await _createUserIfNotExistsUseCase.HandleRequestAsync(new CreateUserIfNotExistsRequest(authUser.UserId));
    }
}

