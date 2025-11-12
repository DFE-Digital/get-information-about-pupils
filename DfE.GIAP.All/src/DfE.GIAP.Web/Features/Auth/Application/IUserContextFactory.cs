using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Application.Models;

namespace DfE.GIAP.Web.Features.Auth.Application;

/// <summary>
/// Builds an AuthenticatedUser from a ClaimsPrincipal.
/// </summary>
public interface IUserContextFactory
{
    AuthenticatedUser FromPrincipal(ClaimsPrincipal principal);
}
