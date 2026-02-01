using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.Common.Application.ValueObjects;

public sealed class LearnerNameTests
{
    [Fact]
    public void Constructor_WithValidFirstLastNames_ShouldInitializeProperties()
    {
        // arrange
        const string firstName = "Alice";
        const string surname = "Smith";

        // act
        LearnerName learnerName = new(firstName, surname);

        // Assert
        Assert.Equal(firstName, learnerName.FirstName);
        Assert.Equal(surname, learnerName.Surname);
    }

    [Fact]
    public void Constructor_WithValidFirstMiddleLastNames_ShouldInitializeProperties()
    {
        // arrange
        const string firstName = "Alice";
        const string middleName = "Beth";
        const string surname = "Smith";

        // act
        LearnerName learnerName = new(firstName, middleName, surname);

        // Assert
        Assert.Equal(firstName, learnerName.FirstName);
        Assert.Equal(middleName, learnerName.MiddleNames);
        Assert.Equal(surname, learnerName.Surname);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public void Constructor_WithValidNames_AndEmptyOrNullMiddleName_Should_NormaliseMiddleName(string? middleNameInput)
    {
        // act
        LearnerName learnerName = new(
            firstName: "John",
            middleName: middleNameInput,
            surname: "Doe");

        // Assert
        Assert.Equal("John", learnerName.FirstName);
        Assert.Equal(string.Empty, learnerName.MiddleNames);
        Assert.Equal("Doe", learnerName.Surname);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public void Constructor_WithInvalidFirstName_ShouldThrowArgumentException(string? invalidFirstName)
    {
        // act
        Func<LearnerName> act = () =>
            new(invalidFirstName!, "ValidSurname");

        // Assert
        Assert.ThrowsAny<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public void Constructor_WithInvalidSurname_ShouldThrowArgumentException(string? invalidSurname)
    {
        // act
        Func<LearnerName> act = () =>
            new("ValidForename"!, invalidSurname!);

        // Assert
        Assert.ThrowsAny<ArgumentException>(act);
    }

    [Fact]
    public void Equality_WithSameFirstLastNames_ShouldBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Doe");
        LearnerName equalityCheckInstanceB = new("John", "Doe");

        // act & Assert
        Assert.Equal(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.True(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithDifferentFirstNames_SameLastNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("Michael", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Doe");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithDifferentLastNames_SameFirstNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Donald");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithSameFirstMiddleLastNames_ShouldBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Knight", "Doe");
        LearnerName equalityCheckInstanceB = new("John", "Knight", "Doe");

        // act & Assert
        Assert.Equal(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.True(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithDifferentFirstNames_SameMiddleLastNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("Michael", "Knight", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Knight", "Doe");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }


    [Fact]
    public void Equality_WithDifferentMiddleNames_SameFirstLastNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Knight", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Lancelot", "Doe");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithDifferentLastNames_SameFirstMiddleNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Knight", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Knight", "Donald");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }
}

