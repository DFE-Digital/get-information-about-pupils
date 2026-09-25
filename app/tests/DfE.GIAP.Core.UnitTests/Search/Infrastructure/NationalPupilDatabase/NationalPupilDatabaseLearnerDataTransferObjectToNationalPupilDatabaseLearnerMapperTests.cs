using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.NationalPupilDatabase;

public sealed class NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapperTests
{
    private readonly NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper _sut;

    public NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapperTests()
    {
        _sut = new NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper();
    }

    [Fact]
    public void Map_Returns_RequestError_When_Input_Is_Null()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject? dto = null;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto!);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.HasResult.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessage.Should().Be("input cannot be null");
        result.Exception.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_Returns_RequestError_When_UPN_Is_Null_Or_Empty(string? upn)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.UPN = upn;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.UPN cannot be null");
        result.HasResult.Should().BeFalse();
        result.Result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_Returns_RequestError_When_Forename_Is_Null_Or_Empty(string? forename)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Forename = forename;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.Forename cannot be null");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_Returns_RequestError_When_Surname_Is_Null_Or_Empty(string? surname)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Surname = surname;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.Surname cannot be null");
    }

    [Fact]
    public void Map_Returns_RequestError_When_DOB_Is_Null()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.DOB = null;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.DOB cannot be null");
    }

    [Fact]
    public void Map_Returns_RequestError_When_LocalAuthority_Is_Null()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.LocalAuthority = null!;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.LocalAuthority cannot be null");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Map_Returns_MappingError_When_LocalAuthority_Is_Empty_Or_Whitespace(string la)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.LocalAuthority = la;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapToError);
        result.HasResult.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Exception.Should().NotBeNull(); // thrown by LocalAuthorityCode ctor (domain)
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Map_Returns_Success_With_Mapped_Learner_When_Input_Is_Valid()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        result.Exception.Should().BeNull();

        NationalPupilDatabaseLearner learner = result.Result!;
        learner.Should().NotBeNull();
    }

    [Fact]
    public void Map_Sets_Empty_MiddleNames_When_MiddleNames_Is_Null()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Middlenames = null;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();

        NationalPupilDatabaseLearner learner = result.Result!;
        learner.Name.MiddleNames.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("m")]
    [InlineData(" M ")]
    public void Map_Uses_Sex_When_Present_Male(string sex)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Sex = sex;
        dto.Gender = "F"; // ignored because Sex provided

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.Result!.Characteristics.Sex.Should().Be(Sex.Male);
    }

    [Theory]
    [InlineData("F")]
    [InlineData("f")]
    [InlineData(" F ")]
    public void Map_Uses_Sex_When_Present_Female(string sex)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Sex = sex;
        dto.Gender = "M"; // ignored because Sex provided

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.Result!.Characteristics.Sex.Should().Be(Sex.Female);
    }

    [Theory]
    [InlineData("O")]
    [InlineData("o")]
    [InlineData(" z ")]
    public void Map_Uses_Sex_When_Present_Unknown(string sex)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Sex = sex;
        dto.Gender = "F";

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.Result!.Characteristics.Sex.Should().Be(Sex.Unknown);
    }

    [Fact]
    public void Map_Falls_Back_To_Gender_When_Sex_Missing_Male()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Sex = null;
        dto.Gender = "M";

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.Result!.Characteristics.Sex.Should().Be(Sex.Male);
    }

    [Fact]
    public void Map_Falls_Back_To_Gender_When_Sex_Missing_Female()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Sex = null;
        dto.Gender = "F";

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.Result!.Characteristics.Sex.Should().Be(Sex.Female);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X")]
    [InlineData("Unknown")]
    public void Map_Defaults_Sex_To_Unknown_When_Sex_And_Gender_Are_Unknown(string? value)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObject dto =
            NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        dto.Sex = value;
        dto.Gender = value;

        // Act
        IMappedResult<NationalPupilDatabaseLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.Result!.Characteristics.Sex.Should().Be(Sex.Unknown);
    }
}
