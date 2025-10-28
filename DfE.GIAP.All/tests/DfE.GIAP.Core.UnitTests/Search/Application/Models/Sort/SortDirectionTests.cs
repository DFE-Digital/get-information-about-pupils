using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Sort;

public sealed class SortDirectionTests
{
    [Theory]
    [InlineData("asc")]
    [InlineData("ASC")]
    [InlineData("Asc")]
    [InlineData("desc")]
    [InlineData("DESC")]
    [InlineData("Desc")]
    public void Constructor_WithValidInput_ShouldNormalizeAndAssign(string input)
    {
        // act
        SortDirection sortDirection = new(input);

        // Assert
        sortDirection.Direction.Should().Be(input.ToLowerInvariant());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_WithNullOrEmptyInput_ShouldThrowArgumentException(string? input)
    {
        // act
        Action act = () => new SortDirection(input!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("up")]
    [InlineData("down")]
    [InlineData("ascending")]
    [InlineData("descending")]
    public void Constructor_WithInvalidInput_ShouldThrowArgumentException(string input)
    {
        // act
        Action act = () => new SortDirection(input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Unknown sort direction '{input.ToLowerInvariant()}'*");
    }

    [Theory]
    [InlineData("asc", true)]
    [InlineData("desc", true)]
    [InlineData("ASC", false)]
    [InlineData("up", false)]
    public void IsValid_ShouldReturnExpectedResult(string input, bool expected)
    {
        // act
        bool result = SortDirection.IsValid(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Create_ShouldReturnNormalizedSortDirection()
    {
        // act
        SortDirection sortDirection = SortDirection.Create("DESC");

        // Assert
        sortDirection.Direction.Should().Be("desc");
    }
}
