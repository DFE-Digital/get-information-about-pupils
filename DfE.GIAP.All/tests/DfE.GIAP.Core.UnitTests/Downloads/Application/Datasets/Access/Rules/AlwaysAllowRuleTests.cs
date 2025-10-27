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
        AuthorisationContextTestDouble context = new()
        {
            Role = "Guest",
            IsDfeUser = false,
            StatutoryAgeLow = 1,
            StatutoryAgeHigh = 99,
            Claims = new[] { "NoAccess" }
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.True(result);
    }
}
