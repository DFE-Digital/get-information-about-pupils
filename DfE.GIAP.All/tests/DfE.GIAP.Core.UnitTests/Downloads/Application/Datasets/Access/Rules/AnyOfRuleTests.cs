using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Availability.Access.Rules.CompositeRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class AnyOfRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_When_AtLeastOneRuleReturnsTrue()
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        IDatasetAccessRule rule1 = DatasetAccessRuleTestDouble.ReturnsFalse();
        IDatasetAccessRule rule2 = DatasetAccessRuleTestDouble.ReturnsTrue();
        IDatasetAccessRule rule3 = DatasetAccessRuleTestDouble.ReturnsFalse();

        AnyOfRule anyOfRule = new(rule1, rule2, rule3);

        // Act
        bool result = anyOfRule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_AllRulesReturnFalse()
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        IDatasetAccessRule rule1 = DatasetAccessRuleTestDouble.ReturnsFalse();
        IDatasetAccessRule rule2 = DatasetAccessRuleTestDouble.ReturnsFalse();

        AnyOfRule anyOfRule = new(rule1, rule2);

        // Act
        bool result = anyOfRule.HasAccess(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_NoRulesProvided()
    {
        // Arrange
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        AnyOfRule anyOfRule = new();

        // Act
        bool result = anyOfRule.HasAccess(context);

        // Assert
        Assert.False(result);
    }
}
