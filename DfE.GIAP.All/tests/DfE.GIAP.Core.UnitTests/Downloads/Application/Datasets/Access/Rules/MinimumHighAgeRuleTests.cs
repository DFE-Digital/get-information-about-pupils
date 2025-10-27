using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class MinimumHighAgeRuleTests
{
    [Theory]
    [InlineData(14, 14, true)]
    [InlineData(14, 15, true)]
    [InlineData(14, 13, false)]
    [InlineData(14, 0, false)]
    [InlineData(14, 100, true)]
    public void CanDownload_ReturnsExpectedResult_BasedOnStatutoryAgeHigh(int threshold, int highAge, bool expected)
    {
        // Arrange
        MinimumHighAgeRule rule = new(threshold);
        AuthorisationContextTestDouble context = new()
        {
            StatutoryAgeHigh = highAge
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.Equal(expected, result);
    }
}
