using System.Security.Claims;

namespace DfE.GIAP.Web.Features.Auth.Application;

/// <summary>
/// Enriches a principal with application-specific claims.
/// </summary>
public interface IClaimsEnricher
{
    Task<ClaimsPrincipal> EnrichAsync(ClaimsPrincipal providerPrincipal);
}
