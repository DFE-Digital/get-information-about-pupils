using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class IsMATUserAccessRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_WhenUserIsMAT()
    {
        // Arrange
        IsMatUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            isMatUser: true);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_WhenRoleIsNotMAT()
    {
        // Arrange
        IsMatUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            isMatUser: false);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }
}
