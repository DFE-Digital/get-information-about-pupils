using System.Security.Claims;
using DfE.GIAP.Core.PreparedDownloads.Application.Enums;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using static DfE.GIAP.Web.Constants.DsiKeys;

namespace DfE.GIAP.Web.Extensions;

public static class ClaimsPrincipalExtension
{
    private static string GetClaimValue(this ClaimsPrincipal principal, string claimType)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    public static bool IsOrganisationEstablishment(this ClaimsPrincipal principal)
    {
        string organisationCategoryId = principal.GetClaimValue(AuthClaimTypes.OrganisationCategoryId);
        return organisationCategoryId == OrganisationCategory.Establishment;
    }

    public static bool IsOrganisationLocalAuthority(this ClaimsPrincipal principal)
    {
        string catId = principal.GetClaimValue(AuthClaimTypes.OrganisationCategoryId);
        return catId == OrganisationCategory.LocalAuthority;
    }

    public static bool IsOrganisationMultiAcademyTrust(this ClaimsPrincipal principal)
    {
        string catId = principal.GetClaimValue(AuthClaimTypes.OrganisationCategoryId);
        return catId == OrganisationCategory.MultiAcademyTrust;
    }

    public static bool IsOrganisationSingleAcademyTrust(this ClaimsPrincipal principal)
    {
        string catId = principal.GetClaimValue(AuthClaimTypes.OrganisationCategoryId);
        return catId == OrganisationCategory.SingleAcademyTrust;
    }

    public static bool IsOrganisationAllAges(this ClaimsPrincipal principal)
    {
        return principal.GetOrganisationHighAge() == 0 && principal.GetOrganisationLowAge() == 0;
    }

    public static bool IsOrganisationEstablishmentWithFurtherEducation(this ClaimsPrincipal principal)
    {
        if (principal.IsOrganisationEstablishment())
        {
            string establishmentTypeId = principal.GetClaimValue(AuthClaimTypes.OrganisationEstablishmentTypeId);

            return establishmentTypeId == EstablishmentType.FurtherEducation ||
                   establishmentTypeId == EstablishmentType.FurtherEducationString;
        }

        return false;
    }

    public static bool IsEstablishmentWithAccessToULNPages(this ClaimsPrincipal principal)
    {
        if (principal.IsOrganisationEstablishment())
        {
            string highAge = principal.GetClaimValue(AuthClaimTypes.OrganisationHighAge);

            if (int.TryParse(highAge, out int age) && age >= 14)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsEstablishmentWithFurtherEducation(this ClaimsPrincipal principal)
    {
        if (principal.IsOrganisationEstablishment())
        {
            string establishmentTypeId = principal.GetClaimValue(AuthClaimTypes.OrganisationEstablishmentTypeId);

            return establishmentTypeId == EstablishmentType.FurtherEducation ||
                   establishmentTypeId == EstablishmentType.FurtherEducationString;
        }

        return false;
    }

    public static bool IsDfeUser(this ClaimsPrincipal principal)
    {
        string organisationName = principal.GetClaimValue(AuthClaimTypes.OrganisationName);
        return organisationName == DsiKeys.Common.DepartmentForEducation;
    }

    public static bool IsAuthenticated(this ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated ?? false;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(AuthRoles.Admin);
    }

    public static bool IsApprover(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(AuthRoles.Approver);
    }

    public static bool IsNormal(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(AuthRoles.User);
    }

    public static string GetSessionId(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(AuthClaimTypes.SessionId);
    }

    public static string GetUserId(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(AuthClaimTypes.UserId);
    }

    public static string GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(ClaimTypes.Email);
    }

    public static string GetOrganisationCategoryID(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(AuthClaimTypes.OrganisationCategoryId);
    }

    public static string GetOrganisationName(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(AuthClaimTypes.OrganisationName);
    }

    public static int GetOrganisationLowAge(this ClaimsPrincipal principal)
    {
        string lowAge = principal.GetClaimValue(AuthClaimTypes.OrganisationLowAge);
        return int.TryParse(lowAge, out int age) ? age : 0; // Default to 0 if parsing fails
    }

    public static int GetOrganisationHighAge(this ClaimsPrincipal principal)
    {
        string highAge = principal.GetClaimValue(AuthClaimTypes.OrganisationHighAge);
        return int.TryParse(highAge, out int age) ? age : 0; // Default to 0 if parsing fails
    }

    public static string GetEstablishmentNumber(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(AuthClaimTypes.EstablishmentNumber);
    }

    public static string GetLocalAuthorityNumberForEstablishment(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(AuthClaimTypes.LocalAuthorityNumber);
    }

    public static string GetLocalAuthorityNumberForLocalAuthority(this ClaimsPrincipal principal)
    {
        // If the user is a local authority, use the EstablishmentNumber claim
        if (principal.IsOrganisationLocalAuthority())
        {
            return principal.GetClaimValue(AuthClaimTypes.EstablishmentNumber);
        }

        // Otherwise, use the LocalAuthorityNumber claim
        return principal.GetClaimValue(AuthClaimTypes.LocalAuthorityNumber);

    }

    public static string GetUniqueReferenceNumber(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(AuthClaimTypes.UniqueReferenceNumber);
    }

    public static string GetUniqueIdentifier(this ClaimsPrincipal principal)
    {
        return principal.GetClaimValue(AuthClaimTypes.UniqueIdentifier);
    }

    public static OrganisationScope GetOrganisationScope(this ClaimsPrincipal principal)
    {
        if (principal.IsOrganisationLocalAuthority())
            return OrganisationScope.LocalAuthority;

        if (principal.IsOrganisationMultiAcademyTrust())
            return OrganisationScope.MultiAcademyTrust;

        if (principal.IsOrganisationSingleAcademyTrust())
            return OrganisationScope.SingleAcademyTrust;

        if (principal.IsOrganisationEstablishment())
            return OrganisationScope.Establishment;

        throw new NotImplementedException();
    }
}
