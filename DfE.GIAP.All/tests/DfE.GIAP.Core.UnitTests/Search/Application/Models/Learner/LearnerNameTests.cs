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
    public void Constructor_WithInvalidFirstName_ShouldThrowArgumentException(string? invalidFirstName)
    {
        // act
        Action act = () =>
            new LearnerName(invalidFirstName!, "ValidSurname");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The Learner's first name field is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidSurname_ShouldThrowArgumentException(string? invalidSurname)
    {
        // act
        Action act = () =>
            new LearnerName("ValidFirstName", invalidSurname!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The Learner's surname field is required.");
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

