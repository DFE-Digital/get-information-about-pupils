using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.CompositeRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class AllOfRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_When_AllRulesReturnTrue()
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        IDatasetAccessRule rule1 = DatasetAccessRuleTestDouble.ReturnsTrue();
        IDatasetAccessRule rule2 = DatasetAccessRuleTestDouble.ReturnsTrue();
        IDatasetAccessRule rule3 = DatasetAccessRuleTestDouble.ReturnsTrue();

        AllOfRule allOfRule = new(rule1, rule2, rule3);

        // Act
        bool result = allOfRule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_AnyRuleReturnsFalse()
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        IDatasetAccessRule rule1 = DatasetAccessRuleTestDouble.ReturnsTrue();
        IDatasetAccessRule rule2 = DatasetAccessRuleTestDouble.ReturnsFalse();

        AllOfRule allOfRule = new(rule1, rule2);

        // Act
        bool result = allOfRule.HasAccess(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsTrue_When_NoRulesProvided()
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        AllOfRule allOfRule = new(); // no rules

        // Act
        bool result = allOfRule.HasAccess(context);

        // Assert
        Assert.True(result);
    }
}
