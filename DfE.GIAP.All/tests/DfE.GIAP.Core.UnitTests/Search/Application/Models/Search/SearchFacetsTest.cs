using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Search;

public sealed class SearchFacetsTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeEmptyFacetCollection()
    {
        // act
        SearchFacets facets = new();

        // Assert
        facets.Facets.Should().NotBeNull();
        facets.Facets.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithValidFacetList_ShouldInitializeFacets()
    {
        // arrange
        List<SearchFacet> facetList =
        [
            new SearchFacet("Region", [new FacetResult("North", 10)]),
            new SearchFacet("Provider", [new FacetResult("College A", 5)])
        ];

        // act
        SearchFacets facets = new(facetList);

        // Assert
        facets.Facets.Should().BeEquivalentTo(facetList);
    }

    [Fact]
    public void Facets_ShouldBeReadOnly()
    {
        // arrange
        List<SearchFacet> facetList =
        [
            new SearchFacet("Subject", [new FacetResult("Math", 3)])
        ];

        SearchFacets facets = new(facetList);

        // act
        IReadOnlyCollection<SearchFacet> readOnlyFacets = facets.Facets;

        // Assert
        Action mutate = () =>
            ((List<SearchFacet>)readOnlyFacets).Add(new SearchFacet("Extra", []));

        mutate.Should().Throw<InvalidCastException>(); // ReadOnlyCollection cannot be cast to List
    }

    [Fact]
    public void Constructor_WithEmptyEnumerable_ShouldInitializeEmptyCollection()
    {
        // act
        SearchFacets facets = new(new List<SearchFacet>());

        // Assert
        facets.Facets.Should().BeEmpty();
    }
}
