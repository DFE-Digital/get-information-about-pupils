using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.TextBasedSearch.Mappers;

public sealed class FilterRequestMapperTests
{
    private readonly FilterRequestMapper _mapper = new();

    [Fact]
    public void Map_WithValidFilterData_ReturnsCorrectFilterRequest()
    {
        // arrange
        FilterData filterData = new()
        {
            Name = "Status",
            Items =
            [
                new FilterDataItem { Value = "Active" },
                new FilterDataItem { Value = "Inactive" }
            ]
        };

        // act
        FilterRequest result = _mapper.Map(filterData);

        // assert
        Assert.NotNull(result);
        Assert.Equal("Status", result.FilterName);
        Assert.Collection(result.FilterValues,
            item => Assert.Equal("Active", item),
            item => Assert.Equal("Inactive", item));
    }

    [Fact]
    public void Map_WithEmptyItems_ReturnsFilterRequestWithEmptyValues()
    {
        // Arrange: simulate minimal input
        FilterData filterData = new()
        {
            Name = "EmptyFilter",
            Items = []
        };

        // Act
        FilterRequest result = _mapper.Map(filterData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EmptyFilter", result.FilterName);
        Assert.Empty(result.FilterValues);
    }

    [Fact]
    public void Map_WithNullInput_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() => _mapper.Map(null));
    }

    [Fact]
    public void Map_WithNullItemValue_MapsToNullObject()
    {
        // Arrange: simulate null value propagation
        FilterData filterData = new()
        {
            Name = "NullableValues",
            Items =
            [
                new FilterDataItem { Value = null }
            ]
        };

        // Act
        FilterRequest result = _mapper.Map(filterData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NullableValues", result.FilterName);
        Assert.Single(result.FilterValues);
        Assert.Null(result.FilterValues[0]);
    }
}
