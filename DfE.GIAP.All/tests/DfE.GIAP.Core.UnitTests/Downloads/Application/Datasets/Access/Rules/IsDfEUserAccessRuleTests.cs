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
        AuthorisationContextTestDouble context = new()
        {
            IsDfeUser = true
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_WhenUserIsNotDfE()
    {
        // Arrange
        IsDfEUserAccessRule rule = new();
        AuthorisationContextTestDouble context = new()
        {
            IsDfeUser = false
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.False(result);
    }
}
