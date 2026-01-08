using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnerCharacteristicsTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // arrange
        DateTime birthDate = new(2005, 6, 1);
        LearnerCharacteristics.Gender gender = LearnerCharacteristics.Gender.Female;

        // act
        LearnerCharacteristics characteristics = new(birthDate, gender);

        // Assert
        characteristics.BirthDate.Should().Be(new DateOfBirth(birthDate));
        characteristics.Sex.Should().Be(gender);
    }

    [Fact]
    public void Constructor_WithInvalidGender_ShouldThrowArgumentException()
    {
        // arrange
        DateTime birthDate = new(2005, 6, 1);
        LearnerCharacteristics.Gender invalidGender = (LearnerCharacteristics.Gender)999;

        // act
        Action act = () => new LearnerCharacteristics(birthDate, invalidGender);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The learner's gender field is invalid.");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // arrange
        DateTime birthDate = new(2005, 6, 1);
        LearnerCharacteristics.Gender gender = LearnerCharacteristics.Gender.Male;

        LearnerCharacteristics equalityCheckInstanceA = new(birthDate, gender);
        LearnerCharacteristics equalityCheckInstanceB = new(birthDate, gender);

        // act & Assert
        equalityCheckInstanceA.Should().Be(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // arrange
        DateTime birthDate = new(2005, 6, 1);

        LearnerCharacteristics equalityCheckInstanceA = new(birthDate, LearnerCharacteristics.Gender.Male);
        LearnerCharacteristics equalityCheckInstanceB = new(birthDate, LearnerCharacteristics.Gender.Other);

        // act & Assert
        equalityCheckInstanceA.Should().NotBe(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeFalse();
    }
}

