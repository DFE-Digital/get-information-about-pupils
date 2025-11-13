using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Auth.Application.Models;

namespace DfE.GIAP.Web.Features.Auth.Infrastructure;

public static class UserContextFactory
{
    public static AuthenticatedUser FromPrincipal(ClaimsPrincipal principal)
    {
        string userId = principal.FindFirst(AuthClaimTypes.UserId)?.Value ?? string.Empty;

        return new AuthenticatedUser
        {
            UserId = userId,
            IsAdmin = principal.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == AuthRoles.Admin),
            IsApprover = principal.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == AuthRoles.Approver),
            IsUser = principal.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == AuthRoles.User)
        };
    }
}
