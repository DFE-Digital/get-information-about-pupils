using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Services;

public sealed class FakeLearnerSearchResponse : IHasSearchResults
{
    public int Count { get; }

    public FakeLearnerSearchResponse(int count) { Count = count; }
}

public sealed class SearchLearnerByNameServiceTests
{
    [Fact]
    public void Constructor_Throws_When_SearchServiceAdapter_Is_Null()
    {
        // Arrange
        Func<SearchLearnerByNameService<FakeLearnerSearchResponse>> construct =
            () => new(searchServiceAdapter: null!);

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task SearchAsync_Returns_InvalidRequest_When_Request_Is_Null()
    {
        // Arrange
        Mock<ISearchServiceAdapter<FakeLearnerSearchResponse, SearchFacets>> adapter = new();
        SearchLearnerByNameService<FakeLearnerSearchResponse> sut = new(adapter.Object);

        // Act
        SearchServiceResponse<FakeLearnerSearchResponse, SearchFacets> response = await sut.SearchAsync(request: null!);

        // Assert
        Assert.Equal(SearchResponseStatus.InvalidRequest, response.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("*")]
    [InlineData("  * ")]
    public async Task SearchAsync_Returns_InvalidRequest_When_SearchKeywords_Is_Null_Whitespace_Or_Wildcarded(string? keywords)
    {
        // Arrange
        Mock<ISearchServiceAdapter<FakeLearnerSearchResponse, SearchFacets>> adapter = new();

        SearchLearnerByNameService<FakeLearnerSearchResponse> sut = new(adapter.Object);

        SearchLearnerByNameRequest request = new(keywords!, SearchCriteriaTestDouble.Stub(), SortOrderTestDouble.Stub());

        // Act
        SearchServiceResponse<FakeLearnerSearchResponse, SearchFacets> response = await sut.SearchAsync(request);

        // Assert
        Assert.Equal(SearchResponseStatus.InvalidRequest, response.Status);
        adapter.Verify(a => a.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_Returns_Success_And_Maps_Results_And_Facets_When_Adapter_Returns_Results()
    {
        // Arrange
        Mock<ISearchServiceAdapter<FakeLearnerSearchResponse, SearchFacets>> adapter = new();
        SearchLearnerByNameService<FakeLearnerSearchResponse> sut = new(adapter.Object);

        SearchFacets facets = new();

        FakeLearnerSearchResponse fakeResponse = new(10);

        SearchServiceAdaptorResponse<FakeLearnerSearchResponse, SearchFacets> searchResults = new()
        {
            Results = fakeResponse,
            FacetResults = facets
        };

        adapter.Setup(a => a.SearchAsync(It.IsAny<SearchServiceAdapterRequest>())).ReturnsAsync(searchResults);

        SearchLearnerByNameRequest request = new("search-keywords", SearchCriteriaTestDouble.Stub(), SortOrderTestDouble.Stub());

        // Act
        SearchServiceResponse<FakeLearnerSearchResponse, SearchFacets> response = await sut.SearchAsync(request);

        // Assert
        Assert.Equal(SearchResponseStatus.Success, response.Status);
        Assert.Equal(searchResults.Results.Count, response.TotalNumberOfResults);
        Assert.Same(fakeResponse, response.LearnerSearchResults);
        Assert.Same(facets, response.FacetedResults);
    }

    [Fact]
    public async Task SearchAsync_Returns_NoResultsFound_When_Adapter_Returns_Empty_Results()
    {
        // Arrange
        Mock<ISearchServiceAdapter<FakeLearnerSearchResponse, SearchFacets>> adapter = new();
        SearchLearnerByNameService<FakeLearnerSearchResponse> sut = new(adapter.Object);

        FakeLearnerSearchResponse fakeResponse = new(count: 0);

        SearchServiceAdaptorResponse<FakeLearnerSearchResponse, SearchFacets> searchResults = new()
        {
            Results = fakeResponse,
            FacetResults = null
        };

        adapter
            .Setup(a => a.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ReturnsAsync(searchResults);

        SearchLearnerByNameRequest request = new("search-keywords", SearchCriteriaTestDouble.Stub(), SortOrderTestDouble.Stub());

        // Act
        SearchServiceResponse<FakeLearnerSearchResponse, SearchFacets> response = await sut.SearchAsync(request);

        // Assert
        Assert.Equal(SearchResponseStatus.NoResultsFound, response.Status);
        Assert.Null(response.LearnerSearchResults);
        Assert.Null(response.FacetedResults);
    }

    [Fact]
    public async Task SearchAsync_Returns_NoResultsFound_When_Adapter_Returns_Null()
    {
        // Arrange
        Mock<ISearchServiceAdapter<FakeLearnerSearchResponse, SearchFacets>> adapter = new();
        SearchLearnerByNameService<FakeLearnerSearchResponse> sut = new(adapter.Object);

        ISearchServiceAdaptorResponse<FakeLearnerSearchResponse, SearchFacets>? nullResults = null;

        adapter
            .Setup(a => a.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ReturnsAsync(nullResults!);

        SearchLearnerByNameRequest request = new("search-keywords", SearchCriteriaTestDouble.Stub(), SortOrderTestDouble.Stub());

        // Act
        SearchServiceResponse<FakeLearnerSearchResponse, SearchFacets> response = await sut.SearchAsync(request);

        // Assert
        Assert.Equal(SearchResponseStatus.NoResultsFound, response.Status);
    }

    [Fact]
    public async Task SearchAsync_Returns_SearchServiceError_When_Adapter_Throws()
    {
        // Arrange
        Mock<ISearchServiceAdapter<FakeLearnerSearchResponse, SearchFacets>> adapter = new();

        adapter
            .Setup(a => a.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ThrowsAsync(new InvalidOperationException("error"));

        SearchLearnerByNameService<FakeLearnerSearchResponse> sut = new(adapter.Object);

        SearchLearnerByNameRequest request = new("search-keywords", SearchCriteriaTestDouble.Stub(), SortOrderTestDouble.Stub());

        // Act
        SearchServiceResponse<FakeLearnerSearchResponse, SearchFacets> response = await sut.SearchAsync(request);

        // Assert
        Assert.Equal(SearchResponseStatus.SearchServiceError, response.Status);
    }

    [Fact]
    public async Task SearchAsync_Passes_Correct_SearchServiceAdapterRequest_To_Adapter()
    {
        // Arrange
        Mock<ISearchServiceAdapter<FakeLearnerSearchResponse, SearchFacets>> adapter = new();

        FakeLearnerSearchResponse resultsFake = new(100);

        SearchServiceAdaptorResponse<FakeLearnerSearchResponse, SearchFacets> searchResultsFake = new()
        {
            Results = resultsFake,
            FacetResults = null
        };

        SearchServiceAdapterRequest? captured = null;
        adapter
            .Setup(a => a.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .Callback<SearchServiceAdapterRequest>((req) => captured = req)
            .ReturnsAsync(searchResultsFake);

        SearchLearnerByNameService<FakeLearnerSearchResponse> sut = new(adapter.Object);

        IList<string> searchFields = ["name", "alias"];

        IList<string> facets = ["region", "year"];


        int requestOffset = 7;

        SearchLearnerByNameRequest request = new(
            searchKeywords: "searchKeywords",
            searchCriteria: SearchCriteriaTestDouble.Stub(),
            SortOrderTestDouble.Stub(),
            filters: [new FilterRequest("region", ["London"])],
            offset: requestOffset);

        // Act
        await sut.SearchAsync(request);

        // Assert
        Assert.NotNull(captured);

        Assert.Equal(request.SearchKeywords, captured.SearchKeyword);
        Assert.Same(request.SearchCriteria.SearchFields, captured.SearchFields);
        Assert.Same(request.SearchCriteria.Facets, captured.Facets);
        Assert.Same(request.FilterRequests, captured.SearchFilterRequests);
        Assert.Equal(request.SortOrder, captured.SortOrdering);

        Assert.Equal(request.SearchCriteria.Index, captured!.Index);
        Assert.Equal(request.SearchCriteria.Size, captured.Size);
        Assert.Equal(request.SearchCriteria.IncludeTotalCount, captured.IncludeTotalCount);
        Assert.Equal(requestOffset, captured.Offset);
    }
}
