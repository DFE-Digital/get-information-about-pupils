using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Mappers;
using Xunit;
using FacetResult = DfE.GIAP.Core.Search.Application.Models.Search.FacetResult;

namespace DfE.GIAP.Web.Tests.Controllers.TextBasedSearch.Mappers;

public sealed class FilterResponseMapperTests
{
    private readonly FilterResponseMapper _mapper = new();

    [Fact]
    public void Map_WithValidSearchFacet_ReturnsCorrectFilterData()
    {
        // Arrange
        SearchFacet searchFacet =
            new(
                facetName: "Region",
                facetResults:
                [
                    new FacetResult(value:"North", count: 10),
                    new FacetResult(value:"South", count : 5 )
                ]
            );

        // Act
        FilterData result = _mapper.Map(searchFacet);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Region", result.Name);
        Assert.Equal(2, result.Items.Count);

        Assert.Collection(result.Items,
            item =>
            {
                Assert.Equal("North", item.Value);
                Assert.Equal(10, item.Count);
            },
            item =>
            {
                Assert.Equal("South", item.Value);
                Assert.Equal(5, item.Count);
            });
    }

    [Fact]
    public void Map_WithEmptyResults_ReturnsFilterDataWithEmptyItems()
    {
        // Arrange
        SearchFacet searchFacet =
            new(
                facetName: "EmptyFacet",
                facetResults: []
            );

        // Act
        FilterData result = _mapper.Map(searchFacet);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EmptyFacet", result.Name);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void Map_WithNullResults_ThrowsNullReferenceException()
    {
        // Arrange
        SearchFacet searchFacet =
            new(
                facetName: "NullResults",
                facetResults: null!
            );

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _mapper.Map(searchFacet));
    }

    [Fact]
    public void Map_WithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _mapper.Map(null));
    }

    [Fact]
    public void Map_WithNullValueInResultItem_MapsToNullValue()
    {
        // Arrange
        SearchFacet searchFacet =
            new(facetName: "NullableValues",
                facetResults: [
                    new FacetResult(value: null!, count:42)
                ]
            );

        // Act
        FilterData result = _mapper.Map(searchFacet);

        // Assert
        Assert.Single(result.Items);
        Assert.Null(result.Items[0].Value);
        Assert.Equal(42, result.Items[0].Count);
    }
}
