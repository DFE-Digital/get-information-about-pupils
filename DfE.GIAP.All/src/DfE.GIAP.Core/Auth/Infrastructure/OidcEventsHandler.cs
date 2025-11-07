using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;

namespace DfE.GIAP.Core.Auth.Infrastructure;

public class OidcEventsHandler
{
    private readonly IClaimsEnricher _enricher;
    private readonly IUserContextFactory _userContextFactory;
    private readonly IUseCaseRequestOnly<CreateUserIfNotExistsRequest> _createUser;
    private readonly IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> _getNews;
    private readonly IUseCaseRequestOnly<UpdateLastLoggedInRequest> _updateLastLogin;
    private readonly ISessionProvider _sessionProvider; // TODO: Update to new way
    private readonly OidcSettings _oidcSettings;
    private readonly SignInApiSettings _signInSettings;

    public OidcEventsHandler(
        IClaimsEnricher enricher,
        IUserContextFactory userContextFactory,
        IUseCaseRequestOnly<CreateUserIfNotExistsRequest> createUser,
        IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> getNews,
        IUseCaseRequestOnly<UpdateLastLoggedInRequest> updateLastLogin,
        ISessionProvider sessionProvider,
        OidcSettings oidcSettings,
        SignInApiSettings signInSettings)
    {
        _enricher = enricher;
        _userContextFactory = userContextFactory;
        _createUser = createUser;
        _getNews = getNews;
        _updateLastLogin = updateLastLogin;
        _sessionProvider = sessionProvider;
        _oidcSettings = oidcSettings;
        _signInSettings = signInSettings;
    }

    public async Task OnTokenValidated(TokenValidatedContext ctx)
    {
        ctx.Properties.IsPersistent = true;
        ctx.Properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(_oidcSettings.SessionTimeoutMinutes);

        var enriched = await _enricher.EnrichAsync(ctx.Principal, _signInSettings, ctx.HttpContext.RequestAborted);
        ctx.Principal = enriched;

        var userInfo = _userContextFactory.FromPrincipal(enriched);

        await _createUser.HandleRequestAsync(new CreateUserIfNotExistsRequest(userInfo.UserId));
        var news = await _getNews.HandleRequestAsync(new GetUnreadUserNewsRequest(userInfo.UserId));
        if (news.HasUpdates)
        {
            _sessionProvider.SetSessionValue("ShowNewsBanner", true);
        }
        await _updateLastLogin.HandleRequestAsync(new UpdateLastLoggedInRequest(userInfo.UserId, DateTime.UtcNow));

        // TODO: Logging and event logging

    }
}
