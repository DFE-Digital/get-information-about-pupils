using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Adapters;

public sealed class SearchServiceAdapterRequestTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    public void Constructor_Throws_When_Index_Is_Null_Or_Whitespace(string? index)
    {
        // Arrange
        Func<SearchServiceAdapterRequest> construct = () => new SearchServiceAdapterRequest(
            index: index!,
            searchKeyword: "term",
            searchFields: ["field1"],
            sortOrdering: SortOrderTestDouble.Stub()!,
            size: 10);

        // Assert
        Assert.ThrowsAny<ArgumentException>(construct);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_Throws_When_SearchKeyword_Is_Null_Or_Whitespace(string? keyword)
    {
        // Arrange
        Func<SearchServiceAdapterRequest> construct = () => new SearchServiceAdapterRequest(
            index: "index-a",
            searchKeyword: keyword!,
            searchFields: ["field1"],
            sortOrdering: SortOrderTestDouble.Stub()!,
            size: 10);

        // Assert
        Assert.ThrowsAny<ArgumentException>(construct);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_Throws_When_SearchFields_Is_Null_Or_Empty(bool useNull)
    {
        // Arrange
        List<string>? fields = useNull ? null : [];

        Func<SearchServiceAdapterRequest> construct = () => new SearchServiceAdapterRequest(
            index: "index-a",
            searchKeyword: "term",
            searchFields: fields!,
            sortOrdering: SortOrderTestDouble.Stub(),
            size: 10);

        // Assert
        Assert.ThrowsAny<ArgumentException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SortOrder_Is_Null()
    {
        // Arrange

        Func<SearchServiceAdapterRequest> construct = () => new SearchServiceAdapterRequest(
            index: "index-a",
            searchKeyword: "term",
            searchFields: ["field1"]!,
            sortOrdering: null!,
            size: 10);

        // Assert
        Assert.ThrowsAny<ArgumentException>(construct);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_Throws_When_Size_Is_Zero_Or_Negative(int size)
    {
        // Arrange
        Func<SearchServiceAdapterRequest> construct = () => new SearchServiceAdapterRequest(
            index: "index-a",
            searchKeyword: "term",
            searchFields: ["field1"],
            sortOrdering: SortOrderTestDouble.Stub(),
            size: size);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(construct);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    public void Constructor_Throws_When_Offset_Is_Negative(int offset)
    {
        // Arrange
        Func<SearchServiceAdapterRequest> construct = () => new SearchServiceAdapterRequest(
            index: "index-a",
            searchKeyword: "term",
            searchFields: ["field1"],
            sortOrdering: SortOrderTestDouble.Stub(),
            size: 10,
            facets: null,
            searchFilterRequests: null,
            includeTotalCount: true,
            offset: offset);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(construct);
    }

    [Fact]
    public void Constructor_Sets_All_Properties_When_Valid()
    {
        // Arrange
        string index = "providers";
        string searchKeyword = "maths";
        IList<string> searchFields = ["name", "code"];
        SortOrder sortOrdering = SortOrderTestDouble.Stub();
        int size = 25;
        IList<string> facets = ["region", "phase"];

        IList<FilterRequest> filters =
            [
                new("region", ["London"] ),
                new("phase", ["Primary"])
            ];
        bool includeTotalCount = false;
        int offset = 5;

        // Act
        SearchServiceAdapterRequest request = new SearchServiceAdapterRequest(
            index,
            searchKeyword,
            searchFields,
            sortOrdering,
            size,
            facets,
            filters,
            includeTotalCount,
            offset);

        // Assert
        Assert.Equal(index, request.Index);
        Assert.Equal(searchKeyword, request.SearchKeyword);
        Assert.Equal(searchFields, request.SearchFields);
        Assert.Equal(facets, request.Facets);
        Assert.Equal(filters, request.SearchFilterRequests);
        Assert.Equal(sortOrdering, request.SortOrdering);
        Assert.Equal(size, request.Size);
        Assert.Equal(includeTotalCount, request.IncludeTotalCount);
        Assert.Equal(offset, request.Offset);
    }

    [Fact]
    public void Constructor_Uses_Defaults_When_Optional_Params_Omitted()
    {
        // Arrange
        string index = "schools";
        string searchKeyword = "science";
        IList<string> searchFields = ["title"];
        SortOrder sortOrdering = SortOrderTestDouble.Stub();
        int size = 15;

        // Act
        SearchServiceAdapterRequest request = new(
            index,
            searchKeyword,
            searchFields,
            sortOrdering,
            size);

        // Assert
        Assert.Equal(index, request.Index);
        Assert.Equal(searchKeyword, request.SearchKeyword);
        Assert.Equal(searchFields, request.SearchFields);
        Assert.Empty(request.Facets);
        Assert.Empty(request.SearchFilterRequests);
        Assert.Equal(sortOrdering, request.SortOrdering);
        Assert.Equal(size, request.Size);
        Assert.True(request.IncludeTotalCount);
        Assert.Equal(0, request.Offset);
    }

    [Fact]
    public void Constructor_Allows_Null_Facets_And_Filters_By_Replacing_With_Empty_Collections()
    {
        // Arrange
        string index = "index-a";
        string searchKeyword = "history";
        IList<string> searchFields = ["field1"];
        SortOrder sortOrdering = SortOrderTestDouble.Stub();
        int size = 10;

        // Act
        SearchServiceAdapterRequest request = new(
            index,
            searchKeyword,
            searchFields,
            sortOrdering,
            size,
            facets: null,
            searchFilterRequests: null);

        // Assert
        Assert.NotNull(request.Facets);
        Assert.Empty(request.Facets);
        Assert.NotNull(request.SearchFilterRequests);
        Assert.Empty(request.SearchFilterRequests);
    }
}
