using System.Security.Claims;
using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace DfE.GIAP.Core.Auth.Application;

public interface IPostTokenValidatedHandler
{
    Task HandleAsync(TokenAuthorisationContext context);
}

public class TokenAuthorisationContext
{
    public ClaimsPrincipal Principal { get; set; }

    public TokenAuthorisationContext(TokenValidatedContext ctx)
    {
        Principal = ctx.Principal!;
    }
}


public class ClaimsEnrichmentHandler : IPostTokenValidatedHandler
{
    private readonly IClaimsEnricher _enricher;

    public ClaimsEnrichmentHandler(IClaimsEnricher enricher)
    {
        _enricher = enricher;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        context.Principal = await _enricher.EnrichAsync(context.Principal);
    }
}


public class UserPostLoginHandler : IPostTokenValidatedHandler
{
    private readonly IUseCaseRequestOnly<CreateUserIfNotExistsRequest> _createUser;
    private readonly IUseCaseRequestOnly<UpdateLastLoggedInRequest> _updateLastLogin;
    private readonly IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> _getNews;
    private readonly IUserContextFactory _userContextFactory;
    //private readonly ISessionProvider _sessionProvider;

    public UserPostLoginHandler(
        IUseCaseRequestOnly<CreateUserIfNotExistsRequest> createUser,
        IUseCaseRequestOnly<UpdateLastLoggedInRequest> updateLastLogin,
        IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> getNews,
        IUserContextFactory userContextFactory
        /*ISessionProvider sessionProvider*/)
    {
        _createUser = createUser;
        _updateLastLogin = updateLastLogin;
        _getNews = getNews;
        _userContextFactory = userContextFactory;
        //_sessionProvider = sessionProvider;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        AuthenticatedUser authUser = _userContextFactory.FromPrincipal(context.Principal);

        // Ensure user exists
        await _createUser.HandleRequestAsync(new CreateUserIfNotExistsRequest(authUser.UserId));

        // Update last login
        await _updateLastLogin.HandleRequestAsync(
            new UpdateLastLoggedInRequest(authUser.UserId, DateTime.UtcNow));

        // Check unread news
        GetUnreadUserNewsResponse news = await _getNews.HandleRequestAsync(new GetUnreadUserNewsRequest(authUser.UserId));
        if (news?.HasUpdates == true)
        {
            //_sessionProvider.SetSessionValue("ShowNewsBanner", true);
        }
    }
}
