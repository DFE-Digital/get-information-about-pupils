using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.UnitTests.Domain.ValueObjects;

public class PupilNameTests
{
    [Fact]
    public void Constructor_WithFirstAndLastNameOnly_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        string firstName = "john";
        string lastName = "doe";

        // Act
        PupilName result = new(firstName, lastName);

        // Assert
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.Surname);
    }


    [Theory]
    [InlineData(null, "Smith")]
    [InlineData("Jane", null)]
    [InlineData("", "Smith")]
    [InlineData("Jane", "")]
    [InlineData("   ", "Smith")]
    [InlineData("Jane", "  \n ")]
    [InlineData("Jane", "   \r\n")]
    public void Constructor_WithNullOrWhitespaceNames_ShouldThrowArgumentException(string? firstName, string? lastName)
    {
        // Arrange Act
#pragma warning disable CA1806 // Do not ignore method results
        Action act = () => new PupilName(firstName!, lastName!);
#pragma warning restore CA1806 // Do not ignore method results

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Theory]
    [InlineData("john", "doe")]
    [InlineData("john", "Doe")]
    [InlineData("John", "doe")]
    public void Equality_WithSameValues_ShouldBeEqual(string firstName, string lastName)
    {
        // Arrange
        PupilName inputName = new(firstName, lastName);
        PupilName comparator = new("John", "Doe");

        // Act Assert
        Assert.Equal(inputName, comparator);
    }

    [Theory]
    [InlineData("john", "dOe")]
    [InlineData("johnathan", "Doe")]
    [InlineData("John", "D")]
    public void Equality_WithDifferentValues_ShouldNotBeEqual(string firstName, string lastName)
    {
        // Arrange
        PupilName inputName = new(firstName, lastName);
        PupilName comparator = new("John", "Doe");

        // Act & Assert
        Assert.NotEqual(inputName, comparator);
    }
}
