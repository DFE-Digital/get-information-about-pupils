using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Helpers.DSIUser;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers;

public class DSIUserHelperTests
{
    [Theory]
    [InlineData(DsiKeys.OrganisationCategory.Establishment, "Establishment")]
    [InlineData(DsiKeys.OrganisationCategory.LocalAuthority, "LocalAuthority")]
    [InlineData(DsiKeys.OrganisationCategory.MultiAcademyTrust, "MultiAcademyTrust")]
    [InlineData(DsiKeys.OrganisationCategory.SingleAcademyTrust, "SingleAcademyTrust")]
    [InlineData(DsiKeys.OrganisationCategory.FurtherEducation, "FurtherEducation")]
    [InlineData("", "")]
    public void GetOrganisationType_correctly_returns_organisation_name(string organisationCategoryId, string name)
    {
        Assert.Equal(name, DSIUserHelper.GetOrganisationType(organisationCategoryId));
    }
}
