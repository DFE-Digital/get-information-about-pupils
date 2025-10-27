using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.CompositeRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class AnyOfRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_When_AtLeastOneRuleReturnsTrue()
    {
        // Arrange
        AuthorisationContextTestDouble context = new();
        DatasetAccessRuleTestDouble rule1 = new(false);
        DatasetAccessRuleTestDouble rule2 = new(true);
        DatasetAccessRuleTestDouble rule3 = new(false);

        AnyOfRule anyOfRule = new(rule1, rule2, rule3);

        // Act
        bool result = anyOfRule.CanDownload(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_AllRulesReturnFalse()
    {
        // Arrange
        AuthorisationContextTestDouble context = new();
        DatasetAccessRuleTestDouble rule1 = new(false);
        DatasetAccessRuleTestDouble rule2 = new(false);

        AnyOfRule anyOfRule = new(rule1, rule2);

        // Act
        bool result = anyOfRule.CanDownload(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_NoRulesProvided()
    {
        // Arrange
        AuthorisationContextTestDouble context = new();
        AnyOfRule anyOfRule = new();

        // Act
        bool result = anyOfRule.CanDownload(context);

        // Assert
        Assert.False(result);
    }
}
