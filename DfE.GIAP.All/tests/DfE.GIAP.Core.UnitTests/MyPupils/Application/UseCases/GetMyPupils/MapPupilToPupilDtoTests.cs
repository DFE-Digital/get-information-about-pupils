using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;

public sealed class MapPupilToMyPupilModelMapperTests
{
    [Fact]
    public void Map_ValidPupil_ReturnsExpectedDto()
    {
        // Arrange
        UniquePupilNumber uniquePupilNumber = UniquePupilNumberTestDoubles.Generate();

        DateTime dob = DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(10);

        Pupil pupil = PupilBuilder.CreateBuilder(uniquePupilNumber)
            .WithFirstName("John")
            .WithSurname("Doe")
            .WithDateOfBirth(dob)
            .WithSex(Sex.Male)
            .WithPupilType(PupilType.PupilPremium)
            .WithLocalAuthorityCode(new LocalAuthorityCode(200))
            .Build();

        PupilToMyPupilsModelMapper mapper = new();

        // Act
        MyPupilsModel result = mapper.Map(pupil);

        // Assert
        Assert.Equal(uniquePupilNumber.Value, result.UniquePupilNumber);
        Assert.Equal("John", result.Forename);
        Assert.Equal("Doe", result.Surname);
        Assert.Equal(dob.ToString("yyyy-MM-dd"), result.DateOfBirth);
        Assert.Equal("M", result.Sex);
        Assert.True(result.IsPupilPremium);
        Assert.Equal(200, result.LocalAuthorityCode);
    }

    [Fact]
    public void Map_ThrowsNull_If_Pupil_Is_Null()
    {
        Pupil? pupil = null;
        PupilToMyPupilsModelMapper mapper = new();

        // Act
        Func<MyPupilsModel> act = () => mapper.Map(pupil!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_PupilWithNullDateOfBirth_ReturnsEmptyDateOfBirth()
    {
        // Arrange
        Pupil pupil = PupilBuilder.CreateBuilder(UniquePupilNumberTestDoubles.Generate())
            .WithDateOfBirth(null!)
            .Build();

        PupilToMyPupilsModelMapper mapper = new();

        // Act
        MyPupilsModel result = mapper.Map(pupil);

        // Assert
        Assert.Equal(string.Empty, result.DateOfBirth);
    }

    [Fact]
    public void Map_PupilWithNullSex_ReturnsEmptySex()
    {
        // Arrange
        Pupil pupil = PupilBuilder.CreateBuilder(UniquePupilNumberTestDoubles.Generate())
            .WithSex(null!)
            .Build();

        PupilToMyPupilsModelMapper mapper = new();

        // Act
        MyPupilsModel result = mapper.Map(pupil);

        // Assert
        Assert.Equal(string.Empty, result.Sex);
    }

    [Fact]
    public void Map_PupilNotPupilPremium_ReturnsFalseForIsPupilPremium()
    {
        // Arrange
        Pupil pupil = PupilBuilder.CreateBuilder(UniquePupilNumberTestDoubles.Generate())
            .WithPupilType(PupilType.NationalPupilDatabase)
            .Build();

        PupilToMyPupilsModelMapper mapper = new();

        // Act
        MyPupilsModel result = mapper.Map(pupil);

        // Assert
        Assert.False(result.IsPupilPremium);
    }
}
