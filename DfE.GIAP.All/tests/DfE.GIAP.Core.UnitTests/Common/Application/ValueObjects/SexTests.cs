using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.Common.Application.ValueObjects;
public sealed class SexTests
{
    [Theory]
    [InlineData('m')]
    [InlineData('M')]
    public void Constructor_WithValidMaleCharacter_ShouldNormaliseAndStoreCorrectly(char input)
    {
        // Act
        Sex sex = new(input);

        // Assert
        Assert.Equal(Sex.Male, sex);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("male")]
    [InlineData("MaLE")]
    [InlineData("m")]
    public void Constructor_WithValidMaleCharacters_ShouldNormaliseAndStoreCorrectly(string input)
    {
        // Act
        Sex sex = new(input);

        // Assert
        Assert.Equal(Sex.Male, sex);
    }

    [Theory]
    [InlineData("F")]
    [InlineData("f")]
    [InlineData("female")]
    [InlineData("FEmAle")]
    public void Constructor_WithValidFemaleCharacters_ShouldNormaliseAndStoreCorrectly(string input)
    {
        // Act
        Sex sex = new(input);

        // Assert
        Assert.Equal(Sex.Female, sex);
    }

    [Theory]
    [InlineData('f')]
    [InlineData('F')]
    public void Constructor_WithValidFemaleCharacter_ShouldNormaliseAndStoreCorrectly(char input)
    {
        // Act
        Sex sex = new(input);

        // Assert
        Assert.Equal(Sex.Female, sex);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("z")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("1")]
    [InlineData(null)]
    [InlineData("Z")]
    public void Constructor_WithInvalidCharacters_ShouldReturnEmptyString(string? input)
    {
        // Act
        Sex sex = new(input);

        // Assert
        Assert.Equal("U", sex.ToString());
        // TODO consider throwing Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Male_ShouldReturnCorrectString()
    {
        // Arrange
        Sex expected = new("M");

        // Act
        Sex result = Sex.Male;

        // Assert
        Assert.Equal(expected.ToString(), result.ToString());
        Assert.Equal("M", result.ToString());
    }

    [Fact]
    public void Female_ShouldReturnCorrectChar()
    {
        // Arrange
        Sex expected = new("F");

        // Act
        Sex result = Sex.Female;

        // Assert
        Assert.Equal(expected.ToString(), result.ToString());
        Assert.Equal("F", result.ToString());
    }

    [Fact]
    public void Equality_ShouldWorkForSameSex()
    {
        // Act & Assert
        Assert.Equal(Sex.Male, Sex.Male);
        Assert.Equal(Sex.Female, Sex.Female);
        Assert.Equal(Sex.Unknown, Sex.Unknown);
    }

    [Fact]
    public void Inequality_ShouldWorkForDifferentSex()
    {
        // Act & Assert
        Assert.NotEqual(Sex.Male, Sex.Female);
        Assert.NotEqual(Sex.Male, Sex.Unknown);
        Assert.NotEqual(Sex.Female, Sex.Unknown);
    }
}
