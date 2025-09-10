using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Adapters;

public sealed class SearchServiceAdapterRequestTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // arrange
        string keyword = "math";
        List<string> fields = ["Subject", "Level"];
        List<string> facets = ["Region", "Provider"];
        SortOrder sortOrder = new("Level", "asc", ["Level", "Subject"]);

        List<FilterRequest> filters =
        [
            new FilterRequest("Region", ["North", "South"])
        ];

        int offset = 5;

        // act
        SearchServiceAdapterRequest request =
            new(keyword, fields, facets, sortOrder, filters, offset);

        // assert
        request.SearchKeyword.Should().Be(keyword);
        request.SearchFields.Should().BeEquivalentTo(fields);
        request.Facets.Should().BeEquivalentTo(facets);
        request.SortOrdering.Should().Be(sortOrder);
        request.SearchFilterRequests.Should().BeEquivalentTo(filters);
        request.Offset.Should().Be(offset);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidKeyword_ShouldThrowArgumentException(string? invalidKeyword)
    {
        // arrange
        List<string> fields = ["Field"];
        List<string> facets = ["Facet"];
        SortOrder sortOrder = new("Field", "asc", ["Field" ]);

        // act
        Action act = () =>
            new SearchServiceAdapterRequest(invalidKeyword!, fields, facets, sortOrder);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*searchKeyword*");
    }

    [Fact]
    public void Constructor_WithEmptySearchFields_ShouldThrowArgumentException()
    {
        // arrange
        string keyword = "test";
        SortOrder sortOrder = new("Field", "asc", ["Field"]);

        // act
        Action act = () =>
            new SearchServiceAdapterRequest(keyword, [], ["Facet"], sortOrder);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*searchFields*");
    }

    [Fact]
    public void Constructor_WithEmptyFacets_ShouldThrowArgumentException()
    {
        // arrange
        string keyword = "test";
        SortOrder sortOrder = new SortOrder("Field", "asc", ["Field"]);

        // act
        Action act = () =>
            new SearchServiceAdapterRequest(keyword, ["Field"], [], sortOrder);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*facets*");
    }

    [Fact]
    public void Constructor_WithNullFilters_ShouldInitializeEmptyList()
    {
        // arrange
        string keyword = "test";
        List<string> fields = ["Field"];
        List<string> facets = ["Facet"];
        SortOrder sortOrder = new("Field", "asc", ["Field"]);

        // act
        SearchServiceAdapterRequest request =
            new(keyword, fields, facets, sortOrder, null);

        // Assert
        request.SearchFilterRequests.Should().NotBeNull();
        request.SearchFilterRequests.Should().BeEmpty();
    }
}

