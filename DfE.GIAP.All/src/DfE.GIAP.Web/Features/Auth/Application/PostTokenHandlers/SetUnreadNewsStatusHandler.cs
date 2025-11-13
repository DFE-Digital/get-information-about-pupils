using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;

public class SetUnreadNewsStatusHandler : IPostTokenValidatedHandler
{
    private readonly IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> _getUnreadUserNewsUseCase;
    private readonly ISessionProvider _sessionProvider;

    public SetUnreadNewsStatusHandler(
        IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> getUnreadUserNewsUseCase,
        ISessionProvider sessionProvider)
    {
        ArgumentNullException.ThrowIfNull(getUnreadUserNewsUseCase);
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _getUnreadUserNewsUseCase = getUnreadUserNewsUseCase;
        _sessionProvider = sessionProvider;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        AuthenticatedUser authUser = UserContextFactory.FromPrincipal(context.Principal);

        GetUnreadUserNewsResponse getUnreadUserNewsResponse = await _getUnreadUserNewsUseCase
            .HandleRequestAsync(new GetUnreadUserNewsRequest(authUser.UserId));

        if (getUnreadUserNewsResponse.HasUpdates)
            _sessionProvider.SetSessionValue(SessionKeys.ShowNewsBannerKey, true);
    }
}

