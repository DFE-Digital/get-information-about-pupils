using DfE.GIAP.Core.Search.Application.Models.Learner;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnerIdentifierTests
{
    [Fact]
    public void Constructor_WithValidULN_ShouldInitializeProperty()
    {
        // Arrange
        string validUln = "1234567890";

        // Act
        LearnerIdentifier identifier = new(validUln);

        // Assert
        identifier.UniqueLearnerNumber.Should().Be(1234567890);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceULN_ShouldThrowArgumentException(string? input)
    {
        // Act
        Action act = () => new LearnerIdentifier(input!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unique-Learner-Number (ULN) must not be null or empty.");
    }

    [Theory]
    [InlineData("123456789")]     // Too short
    [InlineData("12345678901")]   // Too long
    public void Constructor_WithInvalidLength_ShouldThrowArgumentException(string input)
    {
        // Act
        Action act = () => new LearnerIdentifier(input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unique-Learner-Number (ULN) must be exactly 10 digits long.");
    }

    [Theory]
    [InlineData("ABCDEFGHIJ")]
    [InlineData("12345abcde")]
    public void Constructor_WithNonNumericULN_ShouldThrowArgumentException(string input)
    {
        // Act
        Action act = () => new LearnerIdentifier(input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unique-Learner-Number (ULN) must be a valid numeric value.");
    }

    [Fact]
    public void Equality_WithSameULN_ShouldBeEqual()
    {
        // Arrange
        LearnerIdentifier equalityCheckInstanceA = new("1234567890");
        LearnerIdentifier equalityCheckInstanceB = new("1234567890");

        // Act & Assert
        equalityCheckInstanceA.Should().Be(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentULN_ShouldNotBeEqual()
    {
        // Arrange
        LearnerIdentifier equalityCheckInstanceA = new("1234567890");
        LearnerIdentifier equalityCheckInstanceB = new("0987654321");

        // Act & Assert
        equalityCheckInstanceA.Should().NotBe(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeFalse();
    }
}

