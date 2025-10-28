using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class IsAdminUserAccessRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_WhenRoleIsGAIPAdmin()
    {
        // Arrange
        IsAdminUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            role: "GIAPAdmin");

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    // TODO: Chnage to actual constant roles instead of magic strings
    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("GAIPAdmin")] // typo variant
    [InlineData("giapadmin")] // case-sensitive mismatch
    [InlineData("")]
    [InlineData(null)]
    public void CanDownload_ReturnsFalse_WhenRoleIsNotGAIPAdmin(string? role)
    {
        // Arrange
        IsAdminUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            role: role ?? string.Empty);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }
}
