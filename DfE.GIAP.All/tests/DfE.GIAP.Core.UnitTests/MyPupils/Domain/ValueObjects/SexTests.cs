using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;

public class SexTests
{
    [Theory]
    [InlineData('M')]
    [InlineData('F')]
    [InlineData('m')]
    [InlineData('f')]
    public void Constructor_WithValidCharacters_ShouldNormaliseAndStoreCorrectly(char input)
    {
        // Arrange
        string expected = char.ToUpperInvariant(input).ToString();

        // Act
        Sex sex = new(input);

        // Assert
        Assert.Equal(expected, sex.ToString());
    }

    [Theory]
    [InlineData('X')]
    [InlineData('z')]
    [InlineData(' ')]
    [InlineData('\n')]
    [InlineData('\r')]
    [InlineData('1')]
    [InlineData('Z')]
    public void Constructor_WithInvalidCharacters_ShouldThrowArgumentException(char input)
    {
        // Arrange Act
#pragma warning disable CA1806 // Do not ignore method results
        Action act = () => new Sex(input);
#pragma warning restore CA1806 // Do not ignore method results

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Male_ShouldReturnCorrectChar()
    {
        // Arrange
        Sex expected = new('M');

        // Act
        Sex result = Sex.Male;

        // Assert
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void Female_ShouldReturnCorrectChar()
    {
        // Arrange
        Sex expected = new('F');

        // Act
        Sex result = Sex.Female;

        // Assert
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void Equality_ShouldWorkForSameSex()
    {
        // Act & Assert
        Assert.Equal(new('M'), Sex.Male);
    }

    [Fact]
    public void Inequality_ShouldWorkForDifferentSex()
    {
        // Act & Assert
        Assert.NotEqual(Sex.Male, Sex.Female);
    }
}
