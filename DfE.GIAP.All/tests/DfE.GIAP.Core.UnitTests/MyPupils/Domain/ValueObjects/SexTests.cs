using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;
public sealed class SexTests
{
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
        Assert.Equal(string.Empty, sex.ToString());
        // TODO when throwing Assert.Throws<ArgumentException>(act);
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
        Assert.Equal(Sex.Female, Sex.Female);
    }

    [Fact]
    public void Inequality_ShouldWorkForDifferentSex()
    {
        // Act & Assert
        Assert.NotEqual(Sex.Male, Sex.Female);
    }
}
