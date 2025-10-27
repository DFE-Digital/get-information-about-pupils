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
        AuthorisationContextTestDouble context = new() { Role = "GAIPAdmin" };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.True(result);
    }

    // TODO: Chnage to actual constant roles instead of magic strings
    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("GIAPAdmin")] // typo variant
    [InlineData("gaipadmin")] // case-sensitive mismatch
    [InlineData("")]
    [InlineData(null)]
    public void CanDownload_ReturnsFalse_WhenRoleIsNotGAIPAdmin(string? role)
    {
        // Arrange
        IsAdminUserAccessRule rule = new();
        AuthorisationContextTestDouble context = new() { Role = role ?? string.Empty };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.False(result);
    }
}
