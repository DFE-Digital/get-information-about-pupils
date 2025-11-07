using System;
using System.Security.Claims;
using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Auth.Infrastructure;

public class OidcEventsHandler
{
    private readonly IClaimsEnricher _claimsEnricher;
    private readonly IUserContextFactory _userContextFactory;
    private readonly IUseCaseRequestOnly<CreateUserIfNotExistsRequest> _createUserIfNotExistsUseCaes;
    private readonly IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> _getUnreadNewsUseCase;
    private readonly IUseCaseRequestOnly<UpdateLastLoggedInRequest> _updateLastLoggedInUseCase;
    //private readonly ISessionProvider _sessionProvider; // TODO: Update to new way
    private readonly DsiOptions _oidcSettings;

    public OidcEventsHandler(
        IClaimsEnricher enricher,
        IUserContextFactory userContextFactory,
        IUseCaseRequestOnly<CreateUserIfNotExistsRequest> createUserIfNotExistsUseCase,
        IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> getUnreadNewsUseCase,
        IUseCaseRequestOnly<UpdateLastLoggedInRequest> updateLastLoggedInUseCase,
        //ISessionProvider sessionProvider,
        IOptions<DsiOptions> oidcSettings)
    {
        _claimsEnricher = enricher;
        _userContextFactory = userContextFactory;
        _createUserIfNotExistsUseCaes = createUserIfNotExistsUseCase;
        _getUnreadNewsUseCase = getUnreadNewsUseCase;
        _updateLastLoggedInUseCase = updateLastLoggedInUseCase;
        //_sessionProvider = sessionProvider;
        _oidcSettings = oidcSettings.Value;
    }

    public Task OnMessageReceived(MessageReceivedContext ctx)
    {
        bool isSpuriousAuthCbRequest =
           ctx.Request.Path == ctx.Options.CallbackPath &&
           ctx.Request.Method == HttpMethods.Get &&
           !ctx.Request.Query.ContainsKey("code");

        if (isSpuriousAuthCbRequest)
        {
            ctx.HandleResponse();
            ctx.Response.Redirect("/");
        }

        return Task.CompletedTask;
    }

    public Task OnRemoteFailure(RemoteFailureContext ctx)
    {
        ctx.HandleResponse();
        if (ctx.Failure is not null)
            return Task.FromException(ctx.Failure);

        // No exception provided – return a completed task or a default exception
        return Task.FromException(new Exception("Unknown remote failure during OIDC authentication."));

    }

    public async Task OnTokenValidated(TokenValidatedContext ctx)
    {
        if (ctx.Principal is null)
        {
            // TODO: log and bail out
            return;
        }

        ctx.Properties!.IsPersistent = true;
        ctx.Properties!.ExpiresUtc = DateTime.UtcNow.AddMinutes(_oidcSettings.SessionTimeoutMinutes);

        ClaimsPrincipal enrichedClaims = await _claimsEnricher.EnrichAsync(ctx.Principal);
        ctx.Principal = enrichedClaims;

        AuthenticatedUser userInfo = _userContextFactory.FromPrincipal(enrichedClaims);

        await _createUserIfNotExistsUseCaes.HandleRequestAsync(new CreateUserIfNotExistsRequest(userInfo.UserId));
        GetUnreadUserNewsResponse news = await _getUnreadNewsUseCase.HandleRequestAsync(new GetUnreadUserNewsRequest(userInfo.UserId));
        if (news.HasUpdates)
        {
            //_sessionProvider.SetSessionValue("ShowNewsBanner", true);
        }
        await _updateLastLoggedInUseCase.HandleRequestAsync(new UpdateLastLoggedInRequest(userInfo.UserId, DateTime.UtcNow));

        // TODO: Logging and event logging

    }
}
