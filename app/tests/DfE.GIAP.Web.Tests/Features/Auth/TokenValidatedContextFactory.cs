using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Tests.Features.Auth;

public static class TokenValidatedContextFactory
{
    public static TokenValidatedContext Create(
        ClaimsPrincipal? principal = null,
        AuthenticationProperties? properties = null,
        string schemeName = "oidc",
        string callbackPath = "/signin-oidc")
    {
        DefaultHttpContext httpContext = new();
        AuthenticationScheme scheme = new(schemeName, null, typeof(OpenIdConnectHandler));
        OpenIdConnectOptions options = new()
        {
            CallbackPath = callbackPath
        };

        return new TokenValidatedContext(httpContext, scheme, options,
            principal ?? new ClaimsPrincipal(new ClaimsIdentity()),
            properties ?? new AuthenticationProperties());
    }
}
