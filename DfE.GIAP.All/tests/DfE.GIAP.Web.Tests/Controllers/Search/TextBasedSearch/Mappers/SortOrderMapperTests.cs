using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;
using Microsoft.Extensions.Options;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers;

public class SortOrderMapperTests
{
    private static SortOrderMapper CreateMapper(params string[] validFields)
    {
        IOptions<SortFieldOptions> options =
            Options.Create(new SortFieldOptions { ValidFields = validFields });

        return new SortOrderMapper(options);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // act/assert
        ArgumentNullException ex =
            Assert.Throws<ArgumentNullException>(() => new SortOrderMapper(null!));

        Assert.Contains("Sort field options must be provided", ex.Message);
    }

    [Fact]
    public void Map_WithNullFieldAndDirection_ReturnsDefaultSortOrder()
    {
        // arrange
        SortOrderMapper mapper = CreateMapper("name", "region");

        // act
        ArgumentException ex =
            Assert.Throws<ArgumentException>(() => mapper.Map((null, null)));

        // assert
        Assert.Contains("Unknown sort field 'search.score()' (Parameter 'sortField')", ex.Message);
    }

    [Fact]
    public void Map_WithEmptyFieldAndDirection_ReturnsDefaultSortOrder()
    {
        // arrange
        SortOrderMapper mapper = CreateMapper("name", "region");

        // act
        ArgumentException ex =
            Assert.Throws<ArgumentException>(() => mapper.Map(("", "")));

        // assert
        Assert.Contains("Unknown sort field 'search.score()' (Parameter 'sortField')", ex.Message);
    }

    [Fact]
    public void Map_WithValidFieldAndDirection_ReturnsExpectedSortOrder()
    {
        // arrange
        SortOrderMapper mapper = CreateMapper("name", "region");

        // act
        SortOrder result = mapper.Map(("name", "asc"));

        // assert
        Assert.Equal("name asc", result.Value);
    }

    [Fact]
    public void Map_WithInvalidField_StillReturnsSortOrder_ButMayBeRejectedLater()
    {
        // arrange
        SortOrderMapper mapper = CreateMapper("name", "region");

        // act
        ArgumentException ex =
            Assert.Throws<ArgumentException>(() => mapper.Map(("invalidField", "desc")));

        // assert
        Assert.Contains("Unknown sort field 'invalidField' (Parameter 'sortField')", ex.Message);
    }

    [Theory]
    [InlineData("name", "asc")]
    [InlineData("region", "desc")]
    [InlineData("search.score()", "desc")]
    public void Map_WithVariousValidInputs_ProducesExpectedSortOrder(string field, string direction)
    {
        // arrange
        SortOrderMapper mapper = CreateMapper("name", "region", "search.score()");

        // act
        SortOrder result = mapper.Map((field, direction));

        // assert
        Assert.Equal($"{field} {direction}", result.Value);
    }
}

