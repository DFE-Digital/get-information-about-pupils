using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Auth.Infrastructure;


/// <summary>
/// Handles OpenID Connect (OIDC) authentication events and coordinates user-related actions during the authentication
/// process.
/// </summary>
/// <remarks>This class is intended to be used as an event handler for OIDC authentication flows, such as those
/// provided by ASP.NET Core's authentication middleware. It manages tasks such as processing authentication callbacks,
/// handling remote authentication failures, enriching user claims, and updating user information upon successful
/// authentication. The handler relies on injected services to perform user creation, claims enrichment, and other user
/// context operations.</remarks>
public class OidcEventsHandler
{
    private readonly IEnumerable<IPostTokenValidatedHandler> _handlers;
    private readonly DsiOptions _oidcSettings;

    public OidcEventsHandler(
        IEnumerable<IPostTokenValidatedHandler> handlers,
        IOptions<DsiOptions> oidcSettings)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(oidcSettings.Value);
        _handlers = handlers;
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

        TokenAuthorisationContext context = new(ctx);
        foreach (IPostTokenValidatedHandler handler in _handlers)
        {
            await handler.HandleAsync(context);
        }

        // Update the principal in the OIDC context if it was enriched
        ctx.Principal = context.Principal;
    }
}
