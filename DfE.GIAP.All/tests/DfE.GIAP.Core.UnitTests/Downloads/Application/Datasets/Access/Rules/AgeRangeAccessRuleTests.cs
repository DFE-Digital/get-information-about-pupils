using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
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
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            statutoryAgeLow: 5,
            statutoryAgeHigh: 15);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_LowAgeIsBelowMinimum()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            statutoryAgeLow: 1,
            statutoryAgeHigh: 15);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_LowAgeIsAboveMaximum()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            statutoryAgeLow: 11,
            statutoryAgeHigh: 15);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_HighAgeIsBelowMinimum()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
           statutoryAgeLow: 5,
           statutoryAgeHigh: 2);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_HighAgeIsAboveMaximum()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
           statutoryAgeLow: 5,
           statutoryAgeHigh: 30);

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDownload_ReturnsFalse_When_LowAgeEqualsHighAge()
    {
        // Arrange
        AgeRangeAccessRule rule = new(minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25);
        IAuthorisationContext context = AuthorisationContextTestDouble.Create(
            statutoryAgeLow: 10,
            statutoryAgeHigh: 10 // not strictly less
        );

        // Act
        bool result = rule.HasAccess(context);

        // Assert
        Assert.False(result);
    }
}
