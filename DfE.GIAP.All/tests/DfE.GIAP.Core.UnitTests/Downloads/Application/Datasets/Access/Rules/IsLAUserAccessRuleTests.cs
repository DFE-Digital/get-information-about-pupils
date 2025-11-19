using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class IsLAUserAccessRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_WhenUserIsLA()
    {
        // Arrange
        IsAdminUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            isLAUser: true);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_WhenRoleIsNotLA()
    {
        // Arrange
        IsAdminUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            isLAUser: false);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }
}
