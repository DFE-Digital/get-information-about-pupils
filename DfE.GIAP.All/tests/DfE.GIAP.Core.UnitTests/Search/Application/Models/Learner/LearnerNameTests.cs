using DfE.GIAP.Core.Search.Application.Models.Learner;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnerNameTests
{
    [Fact]
    public void Constructor_WithValidNames_ShouldInitializeProperties()
    {
        // arrange
        string firstName = "Alice";
        string surname = "Smith";

        // act
        LearnerName learnerName = new(firstName, surname);

        // Assert
        learnerName.FirstName.Should().Be(firstName);
        learnerName.Surname.Should().Be(surname);
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
    public void Equality_WithSameNames_ShouldBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Doe");
        LearnerName equalityCheckInstanceB = new("John", "Doe");

        // act & Assert
        equalityCheckInstanceA.Should().Be(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Doe");

        // act & Assert
        equalityCheckInstanceA.Should().NotBe(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeFalse();
    }
}

