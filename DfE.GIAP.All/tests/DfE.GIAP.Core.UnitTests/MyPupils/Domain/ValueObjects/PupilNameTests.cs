﻿using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    [InlineData("\r\n ")]
    public void Constructor_WithInvalidFirstName_Should_DefaultToEmpty(string? firstName)
    {
        // Arrange Act
        PupilName pupilName = new (firstName!, "Smith");

        // Assert
        Assert.Equal(string.Empty, pupilName.FirstName);
        Assert.Equal("Smith", pupilName.Surname);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    [InlineData("\r\n ")]
    public void Constructor_WithInvalidSurname_Should_DefaultToEmpty(string? surname)
    {
        // Arrange Act
        PupilName pupilName = new("John", surname!);

        // Assert
        Assert.Equal("John", pupilName.FirstName);
        Assert.Equal(string.Empty, pupilName.Surname);
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
