using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class IsEstablishmentUserAccessRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_WhenUserIsEstablishment()
    {
        // Arrange
        IsAdminUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            isEstablishment: true);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_WhenRoleIsNotEstablishment()
    {
        // Arrange
        IsAdminUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            isEstablishment: false);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }
}
