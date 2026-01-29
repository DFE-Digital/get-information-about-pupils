using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.Options;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Mappers;

public class SortOrderMapperTests
{
    private static SortOrderRequestToSortOrderMapper CreateMapper(params string[] validFields)
    {
        IOptions<SortFieldOptions> options =
            Options.Create(
                new SortFieldOptions
                {
                    SortFields = new()
                    {
                        { "key", validFields }
                    }
                });

        return new SortOrderRequestToSortOrderMapper(options);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // act/assert
        ArgumentNullException ex =
            Assert.Throws<ArgumentNullException>(() => new SortOrderRequestToSortOrderMapper(null!));
    }

    [Fact]
    public void Constructor_WithNullOptionsValue_ThrowsArgumentNullException()
    {
        // act/assert
        ArgumentNullException ex =
            Assert.Throws<ArgumentNullException>(()
                => new SortOrderRequestToSortOrderMapper(OptionsTestDoubles.MockNullOptions<SortFieldOptions>()));
    }

    [Fact]
    public void Map_WithNullFieldAndDirection_ReturnsDefaultSortOrder()
    {
        // arrange
        SortOrderRequestToSortOrderMapper mapper = CreateMapper("name", "region");
        SortOrderRequest request = new("key", (null, null));

        // act
        SortOrder order = mapper.Map(request);

        // assert
        Assert.Equal("search.score() desc", order.Value);
    }

    [Fact]
    public void Map_WithEmptyFieldAndDirection_ReturnsDefaultSortOrder()
    {
        // arrange
        SortOrderRequestToSortOrderMapper mapper = CreateMapper("name", "region");
        SortOrderRequest request = new("key", (string.Empty, string.Empty));

        // act
        SortOrder order = mapper.Map(request);

        // assert
        Assert.Equal("search.score() desc", order.Value);
    }

    [Fact]
    public void Map_WithUnknownKey_Throws()
    {
        // arrange
        SortOrderRequestToSortOrderMapper mapper = CreateMapper("name", "region");
        SortOrderRequest request = new("unknown-key", (string.Empty, string.Empty));

        // act
        Assert.Throws<ArgumentException>(() => mapper.Map(request));
    }

    [Fact]
    public void Map_WithValidFieldAndDirection_ReturnsExpectedSortOrder()
    {
        // arrange
        SortOrderRequestToSortOrderMapper mapper = CreateMapper("name", "region");
        SortOrderRequest request = new("key", ("name", "asc"));

        // act
        SortOrder result = mapper.Map(request);

        // assert
        Assert.Equal("name asc", result.Value);
    }

    [Fact]
    public void Map_WithInvalidField_StillReturnsSortOrder_ButMayBeRejectedLater()
    {
        // arrange
        SortOrderRequestToSortOrderMapper mapper = CreateMapper("name", "region");
        SortOrderRequest request = new("key", ("invalidField", "desc"));

        // act
        ArgumentException ex =
            Assert.Throws<ArgumentException>(() => mapper.Map(request));
    }

    [Theory]
    [InlineData("name", "asc")]
    [InlineData("region", "desc")]
    [InlineData("search.score()", "desc")]
    public void Map_WithVariousValidInputs_ProducesExpectedSortOrder(string field, string direction)
    {
        // arrange
        SortOrderRequestToSortOrderMapper mapper = CreateMapper("name", "region", "search.score()");
        SortOrderRequest request = new("key", (field, direction));

        // act
        SortOrder result = mapper.Map(request);

        // assert
        Assert.Equal($"{field} {direction}", result.Value);
    }
}

