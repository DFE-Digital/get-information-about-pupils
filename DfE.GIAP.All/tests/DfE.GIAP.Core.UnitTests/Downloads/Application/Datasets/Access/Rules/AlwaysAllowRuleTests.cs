using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class AlwaysAllowRuleTests
{
    [Fact]
    public void CanDownload_AlwaysReturnsTrue_RegardlessOfContext()
    {
        // Arrange
        AlwaysAllowRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            role: "Guest",
            isDfeUser: false,
            statutoryAgeLow: 1,
            statutoryAgeHigh: 99);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }
}
