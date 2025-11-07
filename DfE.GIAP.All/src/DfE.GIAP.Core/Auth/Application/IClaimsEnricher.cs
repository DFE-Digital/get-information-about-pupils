using System.Security.Claims;
using DfE.GIAP.Core.Auth.Infrastructure.Config;

namespace DfE.GIAP.Core.Auth.Application;

/// <summary>
/// Enriches a principal with application-specific claims.
/// </summary>
public interface IClaimsEnricher
{
    Task<ClaimsPrincipal> EnrichAsync(
        ClaimsPrincipal providerPrincipal,
        SignInApiSettings signInSettings,
        CancellationToken ct);
}
