using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;
public sealed class AgeRangeTests
{
    [Fact]
    public void Constructor_WithValidValues_SetsPropertiesCorrectly()
    {
        // Arrange Act
        AgeRange range = new(5, 10);

        // Assert
        Assert.Equal(5, range.Low);
        Assert.Equal(10, range.High);
        Assert.Equal(5, range.Range);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(5, -10)]
    [InlineData(-1, -1)]
    public void Constructor_WithNegativeValues_ThrowsArgumentOutOfRangeException(int low, int high)
    {
        // Act Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new AgeRange(low, high));
    }

    [Fact]
    public void Constructor_With_HighAbove999_ThrowsArgumentOutOfRangeException()
    {
        // Act Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new AgeRange(2, 1000));
    }

    [Fact]
    public void IsDefaultedRange_Returns_True_When_LowIsZero_And_HighIsZero()
    {
        // Arrange Act
        AgeRange range = new(low: 0, high: 0);

        // Assert
        Assert.True(range.IsDefaultedRange);
    }

    [Fact]
    public void IsDefaultedRange_Returns_False_When_OnlyLowIsNonZero()
    {
        // Arrange Act
        AgeRange range = new(low: 5, high: 0);

        // Assert
        Assert.False(range.IsDefaultedRange);
    }

    [Fact]
    public void IsDefaultedRange_ReturnsFalse_WhenOnlyHighIsZero()
    {
        // Arrange Act
        AgeRange range = new(low: 0, high: 5);

        // Assert
        Assert.False(range.IsDefaultedRange);
    }

    [Fact]
    public void Range_ReturnsZero_WhenLowEqualsHigh()
    {
        // Arrange Act 
        AgeRange range = new(low: 10, high: 10);

        // Assert
        Assert.Equal(0, range.Range);
    }
}
