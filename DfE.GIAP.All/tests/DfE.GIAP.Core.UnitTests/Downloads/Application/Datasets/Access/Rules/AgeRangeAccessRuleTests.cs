using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules.IndividualRules;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Datasets.Access.Rules;

public sealed class AgeRangeAccessRuleTests
{
    [Fact]
    public void CanDownload_ReturnsTrue_When_AgesAreWithinRange_AndLowIsLessThanHigh()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        AuthorisationContextTestDouble context = new()
        {
            StatutoryAgeLow = 5,
            StatutoryAgeHigh = 15
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_LowAgeIsBelowMinimum()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        AuthorisationContextTestDouble context = new()
        {
            StatutoryAgeLow = 1, // too low
            StatutoryAgeHigh = 15
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_LowAgeIsAboveMaximum()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        AuthorisationContextTestDouble context = new()
        {
            StatutoryAgeLow = 11, // too high
            StatutoryAgeHigh = 15
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_HighAgeIsBelowMinimum()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        AuthorisationContextTestDouble context = new()
        {
            StatutoryAgeLow = 5,
            StatutoryAgeHigh = 2 // too low
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_HighAgeIsAboveMaximum()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        AuthorisationContextTestDouble context = new()
        {
            StatutoryAgeLow = 5,
            StatutoryAgeHigh = 30 // too high
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_LowAgeEqualsHighAge()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        AuthorisationContextTestDouble context = new()
        {
            StatutoryAgeLow = 10,
            StatutoryAgeHigh = 10 // not strictly less
        };

        // Act
        bool result = rule.CanDownload(context);

        // Assert
        Assert.False(result);
    }
}
