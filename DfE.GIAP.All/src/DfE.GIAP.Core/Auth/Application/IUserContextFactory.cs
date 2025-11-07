using System.Security.Claims;
using DfE.GIAP.Core.Auth.Application.Models;

namespace DfE.GIAP.Core.Auth.Application;

/// <summary>
/// Builds an AuthenticatedUser from a ClaimsPrincipal.
/// </summary>
public interface IUserContextFactory
{
    AuthenticatedUser FromPrincipal(ClaimsPrincipal principal);
}
