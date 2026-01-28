using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Mappers;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Mappers;

public sealed class FiltersResponseMapperTests
{
    private readonly Mock<IMapper<SearchFacet, FilterData>> _mockFacetMapper;
    private readonly FiltersResponseMapper _mapper;

    public FiltersResponseMapperTests()
    {
        _mockFacetMapper = new Mock<IMapper<SearchFacet, FilterData>>();
        _mapper = new FiltersResponseMapper(_mockFacetMapper.Object);
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // act & assert
        Assert.Throws<ArgumentNullException>(() => new FiltersResponseMapper(null));
    }

    [Fact]
    public void Map_WithEmptyFacets_ReturnsEmptyList()
    {
        // arrange
        SearchFacets input = new(searchFacets: Array.Empty<SearchFacet>());

        // act
        List<FilterData> result = _mapper.Map(input);

        // assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Map_WithSingleFacet_DelegatesToInjectedMapper()
    {
        // arrange
        SearchFacet facet = new(facetName: "Region", []);
        SearchFacets input = new(searchFacets: new[] { facet });

        FilterData expected = new() { Name = "Region", Items = [] };

        _mockFacetMapper
            .Setup(mapper => mapper.Map(facet))
            .Returns(expected);

        // act
        List<FilterData> result = _mapper.Map(input);

        // assert
        Assert.Single(result);
        Assert.Equal("Region", result[0].Name);
        Assert.Empty(result[0].Items);
    }

    [Fact]
    public void Map_WithMultipleFacets_MapsEachFacetCorrectly()
    {
        // arrange
        SearchFacet facet1 = new(facetName: "Status", []);
        SearchFacet facet2 = new(facetName: "Region", []);

        SearchFacets input = new(searchFacets: new[] { facet1, facet2 });

        FilterData expected1 = new() { Name = "Status", Items = [] };
        FilterData expected2 = new() { Name = "Region", Items = [] };

        _mockFacetMapper.Setup(mapper => mapper.Map(facet1)).Returns(expected1);
        _mockFacetMapper.Setup(mapper => mapper.Map(facet2)).Returns(expected2);

        // act
        List<FilterData> result = _mapper.Map(input);

        // assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Status", result[0].Name);
        Assert.Equal("Region", result[1].Name);
    }
}
