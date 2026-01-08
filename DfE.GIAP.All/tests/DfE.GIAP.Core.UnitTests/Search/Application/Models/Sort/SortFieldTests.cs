using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Sort;

public sealed class SortFieldTests
{
    [Fact]
    public void Constructor_WithValidField_ShouldInitializeCorrectly()
    {
        // arrange
        List<string> validFields = ["Surname", "DOB", "Forename"];
        string inputField = "DOB";

        // act
        SortField sortField = new(inputField, validFields);

        // Assert
        sortField.Field.Should().Be(inputField);
        sortField.ValidFields.Should().BeEquivalentTo(validFields);
        sortField.IsValid("dob").Should().BeTrue(); // Case-insensitive check
    }

    [Fact]
    public void Constructor_WithNullField_ShouldThrowArgumentNullException()
    {
        // arrange
        List<string> validFields = ["Surname", "DOB"];

        // act
        Action act = () => new SortField(null!, validFields);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("sortField");
    }

    [Fact]
    public void Constructor_WithEmptyValidFields_ShouldThrowArgumentException()
    {
        // act
        Action act = () => new SortField("Surname", new List<string>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*")
            .And.ParamName.Should().Be("validSortFields");
    }

    [Fact]
    public void Constructor_WithDuplicateValidFields_ShouldThrowArgumentException()
    {
        // arrange
        List<string> validFields = ["DOB", "dob"];

        // act
        Action act = () => new SortField("DOB", validFields);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*contains duplicate entries*")
            .And.ParamName.Should().Be("validSortFields");
    }

    [Fact]
    public void Constructor_WithInvalidField_ShouldThrowArgumentException()
    {
        // arrange
        List<string> validFields = ["Surname", "DOB"];

        // act
        Action act = () => new SortField("Age", validFields);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown sort field 'Age'*")
            .And.ParamName.Should().Be("sortField");
    }

    [Fact]
    public void IsValid_ShouldReturnTrueForCaseInsensitiveMatch()
    {
        // arrange
        List<string> validFields = ["Surname", "DOB"];
        SortField sortField = new("DOB", validFields);

        // act & Assert
        sortField.IsValid("dob").Should().BeTrue();
        sortField.IsValid("SURNAME").Should().BeTrue();
        sortField.IsValid("Forename").Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldReturnValidSortFieldInstance()
    {
        // arrange
        List<string> validFields = ["Level", "Subject"];

        // act
        SortField sortField = SortField.Create("Level", validFields);

        // Assert
        sortField.Field.Should().Be("Level");
        sortField.ValidFields.Should().BeEquivalentTo(validFields);
    }
}

