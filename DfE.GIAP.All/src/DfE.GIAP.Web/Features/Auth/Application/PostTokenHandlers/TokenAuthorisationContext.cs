using System.Security.Claims;
namespace DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;

public class TokenAuthorisationContext
{
    public ClaimsPrincipal Principal { get; set; }

    public TokenAuthorisationContext(ClaimsPrincipal principal)
    {
        Principal = principal;
    }
}
