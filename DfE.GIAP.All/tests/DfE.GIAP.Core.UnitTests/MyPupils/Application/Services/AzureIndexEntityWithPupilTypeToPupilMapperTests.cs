using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;
public sealed class AzureIndexEntityWithPupilTypeToPupilMapperTests
{
    // TODO create TestDoubles for inputs
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act Assert
        Func<Pupil> act = () => mapper.Map(null!);
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_Creates_Pupil_With_Valid_UniquePupilNumber_And_PupilType()
    {
        // Arrange
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: upn.Value,
            forename: "John",
            surname: "Doe",
            sex: "male",
            dob: new DateTime(2010, 1, 1),
            localAuthority: "123");

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper sut = new();

        // Act
        Pupil result = sut.Map(input);

        // Assert
        Assert.Equal(upn.Value, result.Identifier.Value);
        Assert.True(result.IsOfPupilType(PupilType.NationalPupilDatabase));
        Assert.Equal("John", result.Forename);
        Assert.Equal("Doe", result.Surname);
        Assert.Equal("2010-01-01", result.DateOfBirth); // DateOfBirth.ToString() from VO
        Assert.Equal("M", result.Sex);
        Assert.Equal(123, result.LocalAuthorityCode);
    }

    [Fact]
    public void Map_Normalises_Forename_And_Surname()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: "E000000201901",
            forename: "  John  ",
            surname: "  Doe ",
            sex: "m",
            dob: null,
            localAuthority: "0");

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.PupilPremium);

        AzureIndexEntityWithPupilTypeToPupilMapper sut = new();

        // Act
        Pupil result = sut.Map(input);

        // Assert
        Assert.Equal("John", result.Forename); // Note: PupilName.Normalise: leading char upper + rest unchanged
        Assert.Equal("Doe", result.Surname);
        Assert.True(result.IsOfPupilType(PupilType.PupilPremium));
    }

    [Fact]
    public void Map_Sets_Empty_DateOfBirth_When_DOB_Is_Null()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "John",
            surname: "Doe",
            sex: "male",
            dob: null,
            localAuthority: "123");

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper sut = new();

        // Act
        Pupil result = sut.Map(input);

        // Assert
        Assert.False(result.HasDateOfBirth);
        Assert.Equal(string.Empty, result.DateOfBirth);
    }

    [Fact]
    public void Map_Sex_Male_And_Female_Are_Normalised_To_M_And_F()
    {
        // Arrange
        AzureIndexEntity maleDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "John",
            surname: "Doe",
            sex: "Male",
            dob: null,
            localAuthority: "1");

        AzureIndexEntity femaleDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "Fatima",
            surname: "Khan",
            sex: "female",
            dob: null,
            localAuthority: "2");

        AzureIndexEntityWithPupilType maleInput = new(
            maleDto,
            PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilType femaleInput = new(
            femaleDto,
            PupilType.PupilPremium);

        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act
        Pupil maleResult = mapper.Map(maleInput);
        Pupil femaleResult = mapper.Map(femaleInput);

        // Assert
        Assert.Equal("M", maleResult.Sex);
        Assert.Equal("F", femaleResult.Sex);
    }

    [Fact]
    public void Map_Sex_Unknown_Produces_Empty_String()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "John",
            surname: "Doe",
            sex: "unknown",
            dob: null,
            localAuthority: "123");

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper sut = new();

        // Act
        Pupil result = sut.Map(input);

        // Assert
        Assert.Equal(string.Empty, result.Sex);
    }

    [Fact]
    public void Map_LocalAuthority_NonNumeric_Becomes_Null()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "John",
            surname: "Doe",
            sex: "male",
            dob: null,
            localAuthority: "ABC"); // TryParse fails -> null

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.PupilPremium);

        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act
        Pupil result = mapper.Map(input);

        // Assert
        Assert.Null(result.LocalAuthorityCode);
    }

    [Fact]
    public void Map_LocalAuthority_Valid_Range_0_To_999_Is_Accepted()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "John",
            surname: "Doe",
            sex: "male",
            dob: null,
            localAuthority: "999");

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act
        Pupil result = mapper.Map(input);

        // Assert
        Assert.Equal(999, result.LocalAuthorityCode);
    }

    [Fact]
    public void Map_LocalAuthority_Negative_Throws()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "John",
            surname: "Doe",
            sex: "male",
            dob: null,
            localAuthority: "-1"); // TryParse -> -1, VO should throw

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper sut = new();

        // Act
        Func<Pupil> construct = () => sut.Map(input);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(construct);
    }

    [Fact]
    public void Map_LocalAuthority_Greater_Than_999_Throws()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "John",
            surname: "Doe",
            sex: "male",
            dob: null,
            localAuthority: "1000"); // TryParse -> 1000, VO should throw

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act
        Func<Pupil> construct = () => mapper.Map(input);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(construct);
    }

    [Fact]
    public void Map_Respects_PupilType_Preservation()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "John",
            surname: "Doe",
            sex: "male",
            dob: null,
            localAuthority: "3");

        AzureIndexEntityWithPupilType inputPp = new(sourceDto, PupilType.PupilPremium);

        AzureIndexEntityWithPupilType inputNpd = new(sourceDto, PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act
        Pupil pupilPp = mapper.Map(inputPp);
        Pupil pupilNpd = mapper.Map(inputNpd);

        // Assert
        Assert.True(pupilPp.IsOfPupilType(PupilType.PupilPremium));
        Assert.True(pupilNpd.IsOfPupilType(PupilType.NationalPupilDatabase));
    }

    [Fact]
    public void Map_UniquePupilNumber_Invalid_Throws()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: "INVALID-UPN",
            forename: "John",
            surname: "Doe",
            sex: "male",
            dob: null,
            localAuthority: "3");

        AzureIndexEntityWithPupilType input = new(sourceDto, PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act
        Func<Pupil> construct = () => mapper.Map(input);

        // Assert
        Assert.Throws<ArgumentException>(construct);
    }

    [Fact]
    public void Map_Name_Normalises_Empty_Or_Whitespace_To_Empty_Strings()
    {
        // Arrange
        AzureIndexEntity sourceDto = CreateAzureIndexEntity(
            upn: UniquePupilNumberTestDoubles.Generate().Value,
            forename: "  ",     // whitespace -> ""
            surname: string.Empty,        // empty -> ""
            sex: "male",
            dob: null,
            localAuthority: "10");

        AzureIndexEntityWithPupilType input = new(
            sourceDto,
            PupilType.NationalPupilDatabase);

        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act
        Pupil result = mapper.Map(input);

        // Assert
        Assert.Equal(string.Empty, result.Forename);
        Assert.Equal(string.Empty, result.Surname);
    }

    // ============
    // Test Helpers
    // ============

    private static AzureIndexEntity CreateAzureIndexEntity(
        string upn,
        string forename,
        string surname,
        string? sex,
        DateTime? dob,
        string localAuthority)
    {
        AzureIndexEntity dto = new()
        {
            Score = "0.0",
            UPN = upn,
            Forename = forename,
            Surname = surname,
            Sex = sex,
            Gender = null,
            DOB = dob,
            LocalAuthority = localAuthority,
            id = Guid.NewGuid().ToString()
        };

        return dto;
    }
}
