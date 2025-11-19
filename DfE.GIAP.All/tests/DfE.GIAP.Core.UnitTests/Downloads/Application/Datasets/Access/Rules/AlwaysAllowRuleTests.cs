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
        AlwaysAllowAccessRule rule = new();
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }
}
