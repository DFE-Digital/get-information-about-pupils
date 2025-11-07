using System.Security.Claims;
using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Application.Claims;
using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace DfE.GIAP.Core.Auth.Infrastructure;

public class DfeClaimsEnricher : IClaimsEnricher
{
    private readonly IDfeSignInApiClient _apiClient;
    private readonly SignInApiSettings _signInApiSettings;

    public DfeClaimsEnricher(IDfeSignInApiClient apiClient, IOptions<SignInApiSettings> options)
    {
        _apiClient = apiClient;
        _signInApiSettings = options.Value;
    }

    public async Task<ClaimsPrincipal> EnrichAsync(ClaimsPrincipal claimsPrincipal)
    {
        List<Claim> claims = new List<Claim>();

        string userId = claimsPrincipal.FindFirst("sub")?.Value ?? string.Empty;
        string email = claimsPrincipal.FindFirst("email")?.Value ?? string.Empty;
        claims.Add(new Claim(AuthClaimTypes.UserId, userId));
        claims.Add(new Claim(ClaimTypes.Email, email));

        // Organisation claim comes as JSON
        string orgJson = claimsPrincipal.FindFirst("organisation")?.Value ?? "{}";
        JObject org = JObject.Parse(orgJson);
        string orgId = org["id"]?.ToString() ?? string.Empty;

        // Call DfE Sign-In API
        UserAccess? userAccess = await _apiClient.GetUserInfo(_signInApiSettings.ServiceId, orgId, userId);
        Organisation? organisation = await _apiClient.GetUserOrganisation(userId, orgId);

        // Roles
        if (userAccess?.Roles != null)
        {
            foreach (UserRole role in userAccess.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Code));
            }
        }

        // Organisation claims
        claims.Add(new Claim(AuthClaimTypes.OrganisationId, organisation?.Id ?? string.Empty));
        claims.Add(new Claim(AuthClaimTypes.OrganisationName, organisation?.Name ?? string.Empty));
        claims.Add(new Claim(AuthClaimTypes.OrganisationCategoryId, organisation?.Category?.Id ?? string.Empty));
        claims.Add(new Claim(AuthClaimTypes.OrganisationEstablishmentTypeId, organisation?.EstablishmentType?.Id ?? string.Empty));
        claims.Add(new Claim(AuthClaimTypes.OrganisationLowAge, organisation?.StatutoryLowAge ?? "0"));
        claims.Add(new Claim(AuthClaimTypes.OrganisationHighAge, organisation?.StatutoryHighAge ?? "0"));
        claims.Add(new Claim(AuthClaimTypes.EstablishmentNumber, organisation?.EstablishmentNumber ?? string.Empty));
        claims.Add(new Claim(AuthClaimTypes.LocalAuthorityNumber, organisation?.LocalAuthority?.Code ?? string.Empty));
        claims.Add(new Claim(AuthClaimTypes.UniqueReferenceNumber, organisation?.UniqueReferenceNumber ?? string.Empty));
        claims.Add(new Claim(AuthClaimTypes.UniqueIdentifier, organisation?.UniqueIdentifier ?? string.Empty));
        claims.Add(new Claim(AuthClaimTypes.UKProviderReferenceNumber, organisation?.UKProviderReferenceNumber ?? string.Empty));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "DfE-SignIn"));
    }
}
