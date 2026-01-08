using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class IsDfEUserAccessRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_WhenUserIsDfE()
    {
        // Arrange
        IsDfEUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            isDfeUser: true);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_WhenUserIsNotDfE()
    {
        // Arrange
        IsDfEUserAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
           isDfeUser: false);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }
}
