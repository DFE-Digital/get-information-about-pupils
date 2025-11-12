using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;

public class SetUnreadNewsStatusHandler : IPostTokenValidatedHandler
{
    private readonly IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> _getNews;
    private readonly IUserContextFactory _userContextFactory;
    private readonly ISessionProvider _sessionProvider;

    public SetUnreadNewsStatusHandler(
        IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> getNews,
        IUserContextFactory userContextFactory,
        ISessionProvider sessionProvider)
    {
        _getNews = getNews;
        _userContextFactory = userContextFactory;
        _sessionProvider = sessionProvider;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        AuthenticatedUser authUser = _userContextFactory.FromPrincipal(context.Principal);

        GetUnreadUserNewsResponse news = await _getNews.HandleRequestAsync(new GetUnreadUserNewsRequest(authUser.UserId));
        if (news?.HasUpdates is true)
        {
            _sessionProvider.SetSessionValue(SessionKeys.ShowNewsBannerKey, true);
        }
    }
}

