using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Search;

public sealed class SearchFacetTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // arrange
        string facetName = "Region";

        List<FacetResult> facetResults =
        [
            new FacetResult("North", 10),
            new FacetResult("South", 5)
        ];

        // act
        SearchFacet facet = new(facetName, facetResults);

        // Assert
        facet.Name.Should().Be(facetName);
        facet.Results.Should().BeEquivalentTo(facetResults);
    }

    [Fact]
    public void Constructor_WithEmptyResults_ShouldAllowEmptyList()
    {
        // arrange
        SearchFacet facet = new("Provider", []);

        // Assert
        facet.Name.Should().Be("Provider");
        facet.Results.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullName_ShouldAllowNull()
    {
        // arrange
        List<FacetResult> results = [new FacetResult("Unspecified", 0)];

        // act
        SearchFacet facet = new(null!, results);

        // Assert
        facet.Name.Should().BeNull();
        facet.Results.Should().BeEquivalentTo(results);
    }

    [Fact]
    public void Constructor_WithNullResults_ShouldAllowNull()
    {
        // act
        SearchFacet facet = new("Gender", null!);

        // Assert
        facet.Name.Should().Be("Gender");
        facet.Results.Should().BeNull();
    }
}
