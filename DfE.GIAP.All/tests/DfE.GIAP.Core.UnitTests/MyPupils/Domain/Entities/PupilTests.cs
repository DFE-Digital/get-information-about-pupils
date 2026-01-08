using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.Entities;
public sealed class PupilTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Pupil(
                UniquePupilNumberTestDoubles.Generate(),
                It.IsAny<PupilType>(),
                null!,
                It.IsAny<DateTime>(),
                It.IsAny<Sex>(),
                It.IsAny<LocalAuthorityCode>()));
    }

    [Fact]
    public void IsOfPupilType_ReturnsTrue_WhenTypesMatch()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        PupilType expectedType = PupilType.NationalPupilDatabase;

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithPupilType(expectedType)
            .Build();

        // Act
        bool result = pupil.IsOfPupilType(expectedType);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOfPupilType_ReturnsFalse_WhenTypesDoNotMatch()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        PupilType actualType = PupilType.NationalPupilDatabase;
        PupilType differentPupilType = PupilType.PupilPremium;

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithPupilType(actualType)
            .Build();

        // Act
        bool result = pupil.IsOfPupilType(differentPupilType);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Sex_ReturnsEmptyString_WhenSexIsNull()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithSex(null!)
            .Build();

        // Act
        string result = pupil.Sex;

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Sex_ReturnsValue_WhenSexIsSet()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithSex(Sex.Male)
            .Build();

        // Act
        string result = pupil.Sex;

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
        Assert.Equal(Sex.Male.ToString(), result);
    }

    [Fact]
    public void Forename_ReturnsCorrectValue()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        string expectedForename = "John";

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithFirstName(expectedForename)
            .Build();

        // Act
        string result = pupil.Forename;

        // Assert
        Assert.Equal("John", result);
    }

    [Fact]
    public void Surname_ReturnsCorrectValue()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        string expectedSurname = "Doe";

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithSurname(expectedSurname)
            .Build();

        // Act
        string result = pupil.Surname;

        // Assert
        Assert.Equal("Doe", result);
    }

    [Fact]
    public void LocalAuthorityCode_ReturnsCorrectValue()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        LocalAuthorityCode expectedCode = new(100);

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithLocalAuthorityCode(expectedCode)
            .Build();

        // Act
        int result = pupil.LocalAuthorityCode;

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void DateOfBirth_ReturnsEmptyString_WhenDateOfBirthIsNull()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithDateOfBirth(null)
            .Build();

        // Act
        string result = pupil.DateOfBirth;

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void DateOfBirth_ReturnsValue_WhenDateOfBirthIsSet()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        DateTime dob = DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(10);

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithDateOfBirth(dob)
            .Build();

        // Act
        string result = pupil.DateOfBirth;

        // Assert
        Assert.Equal(dob.ToString("yyyy-MM-dd"), result);
    }

    [Fact]
    public void HasDateOfBirth_ReturnsTrue_WhenDateOfBirthIsSet()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithDateOfBirth(DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(10))
            .Build();

        // Act
        bool result = pupil.HasDateOfBirth;

        // Assert
        Assert.True(result);
    }


    [Fact]
    public void TryCalculateAge_WithValidDateOfBirth_ReturnsCorrectAge()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithDateOfBirth(DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(10))
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

        Pupil pupil = PupilBuilder.CreateBuilder(upn)
            .WithDateOfBirth(null)
            .Build();

        // Act
        bool success = pupil.TryCalculateAge(out int? age);

        // Assert
        Assert.False(success);
        Assert.Null(age);
    }

    [Fact]
    public void Pupils_WithSameIdentifier_AreEqual()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        Pupil pupil1 = PupilBuilder.CreateBuilder(upn).Build();
        Pupil pupil2 = PupilBuilder.CreateBuilder(upn).Build();

        // Act Assert
        Assert.Equal(pupil1, pupil2);
    }

    [Fact]
    public void Pupils_WithDifferentIdentifiers_AreNotEqual()
    {
        // Arrange
        List<UniquePupilNumber> uniquePupilNumbers = UniquePupilNumberTestDoubles.Generate(2);
        Pupil pupil1 = PupilBuilder.CreateBuilder(uniquePupilNumbers[0]).Build();
        Pupil pupil2 = PupilBuilder.CreateBuilder(uniquePupilNumbers[1]).Build();

        // Act Assert
        Assert.NotEqual(pupil1, pupil2);
    }
}
