using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Helpers.DSIUser;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers;

public class DSIUserHelperTests
{
    [Theory]
    [InlineData(true, false, false, Roles.Admin)]
    [InlineData(false, true, false, Roles.Approver)]
    [InlineData(false, false, true, Roles.User)]
    [InlineData(true, true, true, Roles.Admin)]
    [InlineData(false, true, true, Roles.Approver)]
    [InlineData(false, false, false, "")]
    public void GetGIAPUserRole_correctly_returns_role_name(bool isAdmin, bool isApprover, bool isUser, string name)
    {
        Assert.Equal(name, DSIUserHelper.GetGIAPUserRole(isAdmin, isApprover, isUser));
    }

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
