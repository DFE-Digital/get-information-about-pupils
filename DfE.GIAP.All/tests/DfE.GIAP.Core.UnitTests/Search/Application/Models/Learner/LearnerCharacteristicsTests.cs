using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnerCharacteristicsTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // Arrange
        DateTime birthDate = new(2005, 6, 1);
        LearnerCharacteristics.Gender gender = LearnerCharacteristics.Gender.Female;

        // Act
        LearnerCharacteristics characteristics = new(birthDate, gender);

        // Assert
        characteristics.BirthDate.Should().Be(new DateOfBirth(birthDate));
        characteristics.Sex.Should().Be(gender);
    }

    [Fact]
    public void Constructor_WithInvalidGender_ShouldThrowArgumentException()
    {
        // Arrange
        DateTime birthDate = new(2005, 6, 1);
        LearnerCharacteristics.Gender invalidGender = (LearnerCharacteristics.Gender)999;

        // Act
        Action act = () => new LearnerCharacteristics(birthDate, invalidGender);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The learner's gender field is invalid.");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        DateTime birthDate = new(2005, 6, 1);
        LearnerCharacteristics.Gender gender = LearnerCharacteristics.Gender.Male;

        LearnerCharacteristics equalityCheckInstanceA = new(birthDate, gender);
        LearnerCharacteristics equalityCheckInstanceB = new(birthDate, gender);

        // Act & Assert
        equalityCheckInstanceA.Should().Be(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        DateTime birthDate = new(2005, 6, 1);

        LearnerCharacteristics equalityCheckInstanceA = new(birthDate, LearnerCharacteristics.Gender.Male);
        LearnerCharacteristics equalityCheckInstanceB = new(birthDate, LearnerCharacteristics.Gender.Other);

        // Act & Assert
        equalityCheckInstanceA.Should().NotBe(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeFalse();
    }
}

