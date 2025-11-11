using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;

namespace DfE.GIAP.Core.Auth.Application.PostTokenHandlers;

public class SetUnreadNewsStatusHandler : IPostTokenValidatedHandler
{
    private readonly IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> _getNews;
    private readonly IUserContextFactory _userContextFactory;
    //private readonly ISessionProvider _sessionProvider;

    public SetUnreadNewsStatusHandler(
        IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> getNews,
        IUserContextFactory userContextFactory
        /*ISessionProvider sessionProvider*/)
    {
        _getNews = getNews;
        _userContextFactory = userContextFactory;
        //_sessionProvider = sessionProvider;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        AuthenticatedUser authUser = _userContextFactory.FromPrincipal(context.Principal);

        // Check unread news
        GetUnreadUserNewsResponse news = await _getNews.HandleRequestAsync(new GetUnreadUserNewsRequest(authUser.UserId));
        if (news?.HasUpdates == true)
        {
            //_sessionProvider.SetSessionValue("ShowNewsBanner", true);
        }
    }
}

