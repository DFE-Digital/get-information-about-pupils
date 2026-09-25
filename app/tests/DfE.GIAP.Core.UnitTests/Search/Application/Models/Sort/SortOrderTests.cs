using DfE.GIAP.Core.Search.Application.Models.Sort;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Sort;

public class SortOrderTests
{
    [Fact]
    public void Constructor_WithValidFieldAndDirection_ShouldInitializeCorrectly()
    {
        // arrange
        List<string> validFields = ["Surname", "DOB", "Forename"];
        string field = "Surname";
        string direction = "desc";

        // act
        SortOrder sortOrder = new(field, direction, validFields);

        // Assert
        sortOrder.Value.Should().Be("Surname desc");
        sortOrder.ToString().Should().Be("Surname desc");
    }

    [Fact]
    public void Constructor_WithInvalidField_ShouldThrowArgumentException()
    {
        // arrange
        List<string> validFields = ["Surname", "DOB"];

        // act
        Action act = () => new SortOrder("Age", "asc", validFields);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown sort field 'Age'*")
            .And.ParamName.Should().Be("sortField");
    }

    [Fact]
    public void Constructor_WithInvalidDirection_ShouldThrowArgumentException()
    {
        // arrange
        List<string> validFields = ["Surname", "DOB"];

        // act
        Action act = () => new SortOrder("DOB", "upward", validFields);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown sort direction 'upward'*")
            .And.ParamName.Should().Be("direction");
    }

    [Fact]
    public void Create_ShouldReturnValidSortOrderInstance()
    {
        // arrange
        List<string> validFields = ["Level", "Subject"];

        // act
        SortOrder sortOrder = SortOrder.Create("Subject", "ASC", validFields);

        // Assert
        sortOrder.Value.Should().Be("Subject asc");
    }

    [Fact]
    public void ToString_ShouldReturnSameValueAsSortExpression()
    {
        // arrange
        List<string> validFields = ["Region"];
        SortOrder sortOrder = new("Region", "DESC", validFields);

        // act & Assert
        sortOrder.ToString().Should().Be("Region desc");
    }
}

