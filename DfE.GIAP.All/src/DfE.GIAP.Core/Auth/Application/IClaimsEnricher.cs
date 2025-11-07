using System.Security.Claims;
namespace DfE.GIAP.Core.Auth.Application;

/// <summary>
/// Enriches a principal with application-specific claims.
/// </summary>
public interface IClaimsEnricher
{
    Task<ClaimsPrincipal> EnrichAsync(ClaimsPrincipal providerPrincipal);
}
