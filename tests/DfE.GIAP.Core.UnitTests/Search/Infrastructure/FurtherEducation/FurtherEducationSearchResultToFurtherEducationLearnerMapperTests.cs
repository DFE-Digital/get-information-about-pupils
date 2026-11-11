using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.FurtherEducation;

public sealed class FurtherEducationSearchResultToFurtherEducationLearnerMapperTests
{
    private readonly FurtherEducationSearchResultToLearnerMapper _mapper;

    public FurtherEducationSearchResultToFurtherEducationLearnerMapperTests()
    {
        _mapper = new();
    }

    [Fact]
    public void Map_WithValidSearchResult_ReturnsSuccess_WithMappedLearner()
    {
        // Arrange
        FurtherEducationLearnerDataTransferObject dto =
            FurtherEducationLearnerDataTransferObjectTestDouble.Fake();

        // Act
        IMappedResult<FurtherEducationLearner> result = _mapper.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.Successful);
        result.HasResult.Should().BeTrue();
        result.Exception.Should().BeNull();
        result.ErrorMessage.Should().BeEmpty();

        FurtherEducationLearner learner = result.Result!;

        learner.Identifier.UniqueLearnerNumber
            .Should().BeInRange(1000000000, 2146999999);

        learner.Name.FirstName.Should().NotBeNullOrWhiteSpace();
        learner.Name.Surname.Should().NotBeNullOrWhiteSpace();

        learner.Characteristics.BirthDate.Should().NotBeNull();
        learner.Characteristics.Sex.Should().BeOneOf(Sex.Male, Sex.Female, Sex.Unknown);
    }

    [Fact]
    public void Map_WithNullInput_ReturnsRequestError()
    {
        // Arrange
        FurtherEducationLearnerDataTransferObject? dto = null;

        // Act
        IMappedResult<FurtherEducationLearner> result = _mapper.Map(dto!);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.HasResult.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessage.Should().Be("input cannot be null");
    }

    [Fact]
    public void Map_WithNullULN_ReturnsRequestError()
    {
        // Arrange
        FurtherEducationLearnerDataTransferObject dto =
            FurtherEducationLearnerDataTransferObjectTestDouble.Fake();
        dto.ULN = null!;

        // Act
        IMappedResult<FurtherEducationLearner> result = _mapper.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.ULN cannot be null");
        result.HasResult.Should().BeFalse();
    }

    [Fact]
    public void Map_WithNullForename_ReturnsRequestError()
    {
        // Arrange
        FurtherEducationLearnerDataTransferObject dto =
            FurtherEducationLearnerDataTransferObjectTestDouble.Fake();
        dto.Forename = null!;

        // Act
        IMappedResult<FurtherEducationLearner> result = _mapper.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.Forename cannot be null");
    }

    [Fact]
    public void Map_WithNullSurname_ReturnsRequestError()
    {
        // Arrange
        FurtherEducationLearnerDataTransferObject dto =
            FurtherEducationLearnerDataTransferObjectTestDouble.Fake();
        dto.Surname = null!;

        // Act
        IMappedResult<FurtherEducationLearner> result = _mapper.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.Surname cannot be null");
    }

    [Fact]
    public void Map_WithNullDOB_ReturnsRequestError()
    {
        // Arrange
        FurtherEducationLearnerDataTransferObject dto =
            FurtherEducationLearnerDataTransferObjectTestDouble.Fake();
        dto.DOB = null!;

        // Act
        IMappedResult<FurtherEducationLearner> result = _mapper.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapFromArgumentError);
        result.ErrorMessage.Should().Be("input.DOB cannot be null");
    }

    [Fact]
    public void Map_WhenConstructorThrows_ReturnsMappingError()
    {
        // Arrange
        FurtherEducationLearnerDataTransferObject dto =
            FurtherEducationLearnerDataTransferObjectTestDouble.Fake();

        // Force a downstream constructor failure: invalid ULN value
        dto.ULN = "INVALID";

        // Act
        IMappedResult<FurtherEducationLearner> result = _mapper.Map(dto);

        // Assert
        result.Status.Should().Be(MappingResultStatus.MapToError);
        result.HasResult.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Exception.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("must be exactly");
    }
}
