using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Common.Application.ValueObjects;

public sealed class LearnerCharacteristicsTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // arrange
        DateTime birthDate = new(2005, 6, 1);
        Gender gender = Gender.Female;

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
        Gender invalidGender = (Gender)999;

        // act
        Func<LearnerCharacteristics> act = () => new(birthDate, invalidGender);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The learner's gender field is invalid.");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // arrange
        DateTime birthDate = new(2005, 6, 1);
        Gender gender = Gender.Male;

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

        LearnerCharacteristics equalityCheckInstanceA = new(birthDate, Gender.Male);
        LearnerCharacteristics equalityCheckInstanceB = new(birthDate, Gender.Other);

        // act & Assert
        equalityCheckInstanceA.Should().NotBe(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeFalse();
    }
}

