using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Claims;

namespace DfE.GIAP.Web.Helpers.DSIUser;

public static class DSIUserHelper
{
    public static string GetGIAPUserRole(bool isAdmin, bool isApprover, bool isUser)
    {
        if (isAdmin)
            return AuthRoles.Admin;
        if (isApprover)
            return AuthRoles.Approver;
        if (isUser)
            return AuthRoles.User;
        return string.Empty;

    }

    public static string GetOrganisationType(string organisationCategoryId)
    {
        return organisationCategoryId switch
        {
            DsiKeys.OrganisationCategory.Establishment => nameof(DsiKeys.OrganisationCategory.Establishment),
            DsiKeys.OrganisationCategory.LocalAuthority => nameof(DsiKeys.OrganisationCategory.LocalAuthority),
            DsiKeys.OrganisationCategory.MultiAcademyTrust => nameof(DsiKeys.OrganisationCategory.MultiAcademyTrust),
            DsiKeys.OrganisationCategory.SingleAcademyTrust => nameof(DsiKeys.OrganisationCategory.SingleAcademyTrust),
            DsiKeys.OrganisationCategory.FurtherEducation => nameof(DsiKeys.OrganisationCategory.FurtherEducation),
            _ => string.Empty,
        };
    }


}
