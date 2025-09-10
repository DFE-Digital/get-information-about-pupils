using DfE.GIAP.Core.Search.Application.Models.Learner;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnerNameTests
{
    [Fact]
    public void Constructor_WithValidNames_ShouldInitializeProperties()
    {
        // Arrange
        string firstName = "Alice";
        string surname = "Smith";

        // Act
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
        // Act
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
        // Act
        Action act = () =>
            new LearnerName("ValidFirstName", invalidSurname!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The Learner's surname field is required.");
    }

    [Fact]
    public void Equality_WithSameNames_ShouldBeEqual()
    {
        // Arrange
        LearnerName equalityCheckInstanceA = new("John", "Doe");
        LearnerName equalityCheckInstanceB = new("John", "Doe");

        // Act & Assert
        equalityCheckInstanceA.Should().Be(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentNames_ShouldNotBeEqual()
    {
        // Arrange
        LearnerName equalityCheckInstanceA = new("John", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Doe");

        // Act & Assert
        equalityCheckInstanceA.Should().NotBe(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeFalse();
    }
}

