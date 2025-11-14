using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Application;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace DfE.GIAP.Web.Features.Auth.Infrastructure;

public class DfeClaimsEnricher : IClaimsEnricher
{
    private readonly IDfeSignInApiClient _apiClient;
    private readonly DsiOptions _dsiOptions;

    public DfeClaimsEnricher(IDfeSignInApiClient apiClient, IOptions<DsiOptions> options)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        ArgumentNullException.ThrowIfNull(options.Value);
        _apiClient = apiClient;
        _dsiOptions = options.Value;
    }

    public async Task<ClaimsPrincipal> EnrichAsync(ClaimsPrincipal claimsPrincipal)
    {
        List<Claim> claims = new List<Claim>();

        string userId = claimsPrincipal.FindFirst("sub")?.Value ?? string.Empty;
        string email = claimsPrincipal.FindFirst("email")?.Value ?? string.Empty;
        claims.Add(new Claim(AuthClaimTypes.SessionId, Guid.NewGuid().ToString()));
        claims.Add(new Claim(AuthClaimTypes.UserId, userId));
        claims.Add(new Claim(ClaimTypes.Email, email));

        // Organisation claim comes as JSON
        string orgJson = claimsPrincipal.FindFirst("organisation")?.Value ?? "{}";
        JObject org = JObject.Parse(orgJson);
        string orgId = org["id"]?.ToString() ?? string.Empty;

        // Call DfE Sign-In API
        UserAccess? userAccess = await _apiClient.GetUserInfo(_dsiOptions.ServiceId, orgId, userId);
        Organisation? organisation = await _apiClient.GetUserOrganisation(userId, orgId);

        //TODO: Current process is to do the below, not sure if this is needed or what purpose it serves?
        //if (userAccess is null)
        //{
        //    eventLogging.TrackEvent(2502, "User log in unsuccessful - user not associated with GIAP service", userId, sessionId, hostEnvironment.ContentRootPath);
        //    ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "DfE-SignIn"));
        //    ctx.HttpContext.Response.Redirect(Routes.Application.UserWithNoRole);
        //    return;
        //}

        // Roles
        if (userAccess?.Roles is not null)
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
