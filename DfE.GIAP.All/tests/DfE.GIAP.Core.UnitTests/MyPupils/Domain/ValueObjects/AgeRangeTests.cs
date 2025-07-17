using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;
public sealed class AgeRangeTests
{
    [Fact]
    public void Constructor_WithValidValues_SetsPropertiesCorrectly()
    {
        // Arrange Act
        AgeLimit range = new(5, 10);

        // Assert
        Assert.Equal(5, range.Low);
        Assert.Equal(10, range.High);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(5, -10)]
    [InlineData(-1, -1)]
    public void Constructor_WithNegativeValues_ThrowsArgumentOutOfRangeException(int low, int high)
    {
        // Act Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new AgeLimit(low, high));
    }

    [Fact]
    public void Constructor_With_HighAbove999_ThrowsArgumentOutOfRangeException()
    {
        // Act Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new AgeLimit(2, 1000));
    }

    [Fact]
    public void IsDefaultedRange_Returns_True_When_LowIsZero_And_HighIsZero()
    {
        // Arrange Act
        AgeLimit range = new(low: 0, high: 0);

        // Assert
        Assert.True(range.IsDefaultLimit);
    }

    [Fact]
    public void IsDefaultedRange_Returns_False_When_OnlyLowIsNonZero()
    {
        // Arrange Act
        AgeLimit range = new(low: 5, high: 0);

        // Assert
        Assert.False(range.IsDefaultLimit);
    }

    [Fact]
    public void IsDefaultedRange_ReturnsFalse_WhenOnlyHighIsZero()
    {
        // Arrange Act
        AgeLimit range = new(low: 0, high: 5);

        // Assert
        Assert.False(range.IsDefaultLimit);
    }
}
