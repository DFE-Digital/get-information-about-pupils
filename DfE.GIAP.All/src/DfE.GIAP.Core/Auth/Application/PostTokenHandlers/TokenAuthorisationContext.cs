using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace DfE.GIAP.Core.Auth.Application.PostTokenHandlers;

public class TokenAuthorisationContext
{
    public ClaimsPrincipal Principal { get; set; }

    public TokenAuthorisationContext(TokenValidatedContext ctx)
    {
        Principal = ctx.Principal!;
    }
}
