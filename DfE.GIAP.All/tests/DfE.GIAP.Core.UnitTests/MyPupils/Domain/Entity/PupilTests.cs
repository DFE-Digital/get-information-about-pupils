using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.Entity;

public sealed class PupilTests
{
    [Fact]
    public void TryCalculateAge_WithValidDateOfBirth_ReturnsCorrectAge()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithDateOfBirth(DateTimeTestDoubles.GenerateDateOfBirthThatHasAlreadyOccuredThisYear()) // Birthday already occurred this year
            .Build();

        // Act
        bool success = pupil.TryCalculateAge(out int? age);

        // Assert
        Assert.True(success);
        Assert.Equal(10, age);
    }

    [Fact]
    public void TryCalculateAge_WithBirthdayNotYetOccurredThisYear_DecrementsAge()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithDateOfBirth(DateTimeTestDoubles.GenerateDateOfBirthThatHasNotOccuredYetThisYear())
            .Build();

        // Act
        bool success = pupil.TryCalculateAge(out int? age);

        // Assert
        Assert.True(success);
        Assert.Equal(9, age);
    }

    [Fact]
    public void TryCalculateAge_WithNullDateOfBirth_ReturnsFalse()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil = // Do not supply a DateOfBirth
            PupilBuilder.CreateBuilder(upn)
            .Build();

        // Act
        bool success = pupil.TryCalculateAge(out int? age);

        // Assert
        Assert.False(success);
        Assert.Null(age);
    }
}
