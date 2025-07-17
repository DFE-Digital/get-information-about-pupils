using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.Entity;

public sealed class PupilTests
{
    private const string MASKED_UPN = "*************";
    [Fact]
    public void TryCalculateAge_WithValidDateOfBirth_ReturnsCorrectAge()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        PupilAuthorisationContext context = MyPupilsAuthorisationContextTestDoubles.Default();

        Pupil pupil = PupilBuilder.CreateBuilder(upn, context)
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
        PupilAuthorisationContext context = MyPupilsAuthorisationContextTestDoubles.Default();

        Pupil pupil = PupilBuilder.CreateBuilder(upn, context)
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
        PupilAuthorisationContext context = MyPupilsAuthorisationContextTestDoubles.Default();

        Pupil pupil = // Do not supply a DateOfBirth
            PupilBuilder.CreateBuilder(upn, context)
            .Build();

        // Act
        bool success = pupil.TryCalculateAge(out int? age);

        // Assert
        Assert.False(success);
        Assert.Null(age);
    }

    [Fact]
    public void UniquePupilNumber_ShouldBeMasked_WhenAgeIsOutOfRange()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        PupilAuthorisationContext context = MyPupilsAuthorisationContextTestDoubles.Generate(
            low: 5,
            high: 19,
            isAdministrator: false);

        Pupil pupil = PupilBuilder
            .CreateBuilder(upn, context)
            .WithDateOfBirth(DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(20))
            .Build();

        // Act
        string result = pupil.UniquePupilNumber;

        // Assert
        Assert.Equal(MASKED_UPN, result);
    }

    [Fact]
    public void UniquePupilNumber_ShouldBeMasked_WhenDateOfBirth_IsUnknown()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        PupilAuthorisationContext context = MyPupilsAuthorisationContextTestDoubles.Generate(
            low: 5,
            high: 19,
            isAdministrator: false);

        Pupil pupil = PupilBuilder
            .CreateBuilder(upn, context) // do not set DoB
            .Build();

        // Act
        string result = pupil.UniquePupilNumber;

        // Assert
        Assert.Equal(MASKED_UPN, result);
    }

    [Fact]
    public void UniquePupilNumber_ShouldNotBeMasked_WhenAgeIsInRange()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        PupilAuthorisationContext context = MyPupilsAuthorisationContextTestDoubles.Generate(
            low: 5,
            high: 19,
            isAdministrator: false);

        Pupil pupil = PupilBuilder
            .CreateBuilder(upn, context)
            .WithDateOfBirth(DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(6))
            .Build();

        // Act
        string result = pupil.UniquePupilNumber;

        // Assert
        Assert.Equal(upn.Value, result);
    }

    [Fact]
    public void UniquePupilNumber_ShouldNotBeMasked_WhenRangeIsDefaulted()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        PupilAuthorisationContext context = MyPupilsAuthorisationContextTestDoubles.GenerateWithNotSetAgeRange();

        Pupil pupil = PupilBuilder
            .CreateBuilder(upn, context)
            .WithDateOfBirth(DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(20))
            .Build();

        // Act
        string result = pupil.UniquePupilNumber;

        // Assert
        Assert.Equal(upn.Value, result);
    }

    [Fact]
    public void UniquePupilNumber_ShouldNotBeMasked_When_IsAdministrator()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        PupilAuthorisationContext context = MyPupilsAuthorisationContextTestDoubles.GenerateAsAdminisrativeUser();

        Pupil pupil = PupilBuilder
            .CreateBuilder(upn, context)
            .Build();

        // Act
        string result = pupil.UniquePupilNumber;

        // Assert
        Assert.Equal(upn.Value, result);
    }
}
