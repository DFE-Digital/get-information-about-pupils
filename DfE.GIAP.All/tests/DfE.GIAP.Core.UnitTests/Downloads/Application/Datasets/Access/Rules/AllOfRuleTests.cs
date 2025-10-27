using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.CompositeRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class AllOfRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_When_AllRulesReturnTrue()
    {
        // Arrange
        AuthorisationContextTestDouble context = new();
        DatasetAccessRuleTestDouble rule1 = new(true);
        DatasetAccessRuleTestDouble rule2 = new(true);
        DatasetAccessRuleTestDouble rule3 = new(true);

        AllOfRule allOfRule = new(rule1, rule2, rule3);

        // Act
        bool result = allOfRule.CanDownload(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_AnyRuleReturnsFalse()
    {
        // Arrange
        AuthorisationContextTestDouble context = new();
        DatasetAccessRuleTestDouble rule1 = new(true);
        DatasetAccessRuleTestDouble rule2 = new(false);

        AllOfRule allOfRule = new(rule1, rule2);

        // Act
        bool result = allOfRule.CanDownload(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsTrue_When_NoRulesProvided()
    {
        // Arrange
        AuthorisationContextTestDouble context = new();
        AllOfRule allOfRule = new(); // no rules

        // Act
        bool result = allOfRule.CanDownload(context);

        // Assert
        Assert.True(result);
    }
}
