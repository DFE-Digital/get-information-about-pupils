using DfE.GIAP.Web.Constants;

namespace DfE.GIAP.Web.Helpers.DSIUser;

public static class DSIUserHelper
{
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
