using DfE.GIAP.Core.Common.Application.ValueObjects;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Common.Application.ValueObjects;

public sealed class LearnerCharacteristicsTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // arrange
        DateTime birthDate = new(2005, 6, 1);
        Sex gender = Sex.Female;

        // act
        LearnerCharacteristics characteristics = new(birthDate, gender);

        // Assert
        characteristics.BirthDate.Should().Be(new DateOfBirth(birthDate));
        characteristics.Sex.Should().Be(gender);
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // arrange
        DateTime birthDate = new(2005, 6, 1);
        Sex gender = Sex.Male;

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

        LearnerCharacteristics equalityCheckInstanceA = new(birthDate, Sex.Male);
        LearnerCharacteristics equalityCheckInstanceB = new(birthDate, Sex.Unknown);

        // act & Assert
        equalityCheckInstanceA.Should().NotBe(equalityCheckInstanceB);
        equalityCheckInstanceA.Equals(equalityCheckInstanceB).Should().BeFalse();
    }
}

