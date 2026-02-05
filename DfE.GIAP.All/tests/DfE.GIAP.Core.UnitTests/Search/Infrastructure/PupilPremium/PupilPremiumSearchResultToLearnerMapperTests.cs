using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using FluentAssertions;

public sealed class PupilPremiumSearchResultToLearnerMapperTests
{
    private readonly PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper _sut;

    public PupilPremiumSearchResultToLearnerMapperTests()
    {
        _sut = new PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper();
    }

    private static PupilPremiumLearnerDataTransferObject CreateStub()
        => PupilPremiumLearnerDataTransferObjectTestDoubles.Stub();

    [Fact]
    public void Map_Returns_RequestError_When_Input_Is_Null()
    {
        // Arrange
        PupilPremiumLearnerDataTransferObject? dto = null;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto!);

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
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.UPN = upn;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

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
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.Forename = forename;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

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
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.Surname = surname;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.Surname cannot be null");
    }

    [Fact]
    public void Map_Returns_RequestError_When_DOB_Is_Null()
    {
        // Arrange
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.DOB = null;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.DOB cannot be null");
    }

    [Fact]
    public void Map_Returns_RequestError_When_LocalAuthority_Is_Null()
    {
        // Arrange
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.LocalAuthority = null!;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.LocalAuthority cannot be null");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Map_Returns_MappingError_When_LocalAuthority_Is_Empty_Or_Whitespace(string localAuthority)
    {
        // Arrange
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.LocalAuthority = localAuthority;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapToError);
        result.HasResult.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Exception.Should().NotBeNull();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Map_Returns_Success_With_Mapped_Learner_When_Input_Is_Valid()
    {
        // Arrange
        PupilPremiumLearnerDataTransferObject dto = CreateStub();

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        result.Exception.Should().BeNull();

        PupilPremiumLearner learner = result.Result!;
        learner.Should().NotBeNull();
    }

    [Fact]
    public void Map_Sets_Empty_MiddleNames_When_MiddleNames_Is_Null()
    {
        // Arrange
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.Middlenames = null;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();

        PupilPremiumLearner learner = result.Result!;
        learner.Name.MiddleNames.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("m")]
    [InlineData(" M ")]
    public void Map_Uses_Sex_When_Present_Male(string sex)
    {
        // Arrange
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.Sex = sex;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

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
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.Sex = sex;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.Result!.Characteristics.Sex.Should().Be(Sex.Female);
    }

    [Theory]
    [InlineData("O")]   // other/unknown code
    [InlineData("o")]
    [InlineData(" z ")] // unexpected
    [InlineData(null)]  // null -> Unknown (assuming Sex VO handles null/whitespace as Unknown)
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X")]
    [InlineData("Unknown")]
    public void Map_Sets_Sex_To_Unknown_When_Sex_Is_Null_Empty_Or_Unknown(string? sex)
    {
        // Arrange
        PupilPremiumLearnerDataTransferObject dto = CreateStub();
        dto.Sex = sex;

        // Act
        IMappedResult<PupilPremiumLearner> result = _sut.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.Result!.Characteristics.Sex.Should().Be(Sex.Unknown);
    }
}
