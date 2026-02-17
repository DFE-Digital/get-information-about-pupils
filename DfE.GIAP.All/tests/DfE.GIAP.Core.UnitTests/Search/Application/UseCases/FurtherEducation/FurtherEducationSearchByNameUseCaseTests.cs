using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.FurtherEducation;

public sealed class FurtherEducationSearchUseCaseTests
{
    private readonly SearchCriteria _furtherEducationSearchCriteriaStub;

    public FurtherEducationSearchUseCaseTests()
    {
        // Arrange (shared stubs)
        _furtherEducationSearchCriteriaStub = SearchCriteriaTestDouble.Stub();
    }

    [Fact]
    public void Constructor_Throws_When_Service_Is_Null()
    {
        // Arrange
        Func<FurtherEducationSearchByNameUseCase> construct =
            () => new FurtherEducationSearchByNameUseCase(searchForLearnerByNameService: null!);

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequest_NullRequest_Throws_ArgumentNullException()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<FurtherEducationLearners>> mockService = new();

        FurtherEducationSearchByNameUseCase useCase = new(mockService.Object);

        // Act
        Func<Task> act = async () => _ = await useCase.HandleRequestAsync(request: null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequest_ServiceThrows_ExceptionBubbles()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<FurtherEducationLearners>> mockService = new();

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ThrowsAsync(new ApplicationException("service failure"));

        FurtherEducationSearchByNameUseCase useCase = new(mockService.Object);

        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _furtherEducationSearchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub()
        };

        // Act
        Func<Task> act = async () => _ = await useCase.HandleRequestAsync(request);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>();
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleRequest_NoResults_Still_Returns_Response_With_Empty_Collections_And_Total()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<FurtherEducationLearners>> mockService = new();

        ISearchResults<FurtherEducationLearners, SearchFacets> noResults =
            FurtherEducationSearchResultsTestDoubles.StubWithNoResults();

        SearchServiceResponse<FurtherEducationLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.NoResultsFound, 0)
            {
                LearnerSearchResults = noResults.Results!,
                FacetedResults = noResults.FacetResults
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ReturnsAsync(serviceResponse);

        FurtherEducationSearchByNameUseCase useCase = new(mockService.Object);

        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _furtherEducationSearchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub()
        };

        // Act
        SearchResponse<FurtherEducationLearners> response = await useCase.HandleRequestAsync(request);

        // Verify
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);

        // Assert
        response.TotalNumberOfResults.Count.Should().Be(0);
        response.LearnerSearchResults!.Learners.Should().BeEmpty();
        response.FacetedResults.Facets.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_CallsServiceWithMappedRequestParams()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<FurtherEducationLearners>> mockService = new();

        SearchLearnerByNameRequest? captured = null;

        // Return a minimal successful response from the service
        ISearchResults<FurtherEducationLearners, SearchFacets> stub = FurtherEducationSearchResultsTestDoubles.Stub();
        FurtherEducationLearners learners = stub.Results!;
        SearchFacets facets = stub.FacetResults!;
        int totalNumberOfResults = 42;

        SearchServiceResponse<FurtherEducationLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.Success, totalNumberOfResults)
            {
                LearnerSearchResults = learners,
                FacetedResults = facets
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .Callback<SearchLearnerByNameRequest>(req => captured = req)
            .ReturnsAsync(serviceResponse);

        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _furtherEducationSearchCriteriaStub,
            FilterRequests = [FilterRequestTestDouble.Fake()],
            SortOrder = SortOrderTestDouble.Stub(),
            Offset = 7
        };

        FurtherEducationSearchByNameUseCase useCase = new(mockService.Object);

        // Act
        SearchResponse<FurtherEducationLearners> response =
            await useCase.HandleRequestAsync(request);

        // Verify
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);

        // Assert (mapping into service request)
        captured.Should().NotBeNull();
        captured!.SearchKeywords.Should().Be(request.SearchKeywords);
        captured.SearchCriteria.Should().BeSameAs(request.SearchCriteria);
        captured.SortOrder.Should().Be(request.SortOrder);
        captured.FilterRequests.Should().BeEquivalentTo(request.FilterRequests);
        captured.Offset.Should().Be(request.Offset);

        // Assert (returned response mirrors service response)
        response.LearnerSearchResults.Should().BeSameAs(learners);
        response.FacetedResults.Should().BeSameAs(facets);
        response.TotalNumberOfResults.Count.Should().Be(totalNumberOfResults);
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_ReturnsResponse_From_Service()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<FurtherEducationLearners>> mockService = new();

        // Use the shared doubles to produce consistent structures for assertions
        ISearchResults<FurtherEducationLearners, SearchFacets> stubResults =
            FurtherEducationSearchResultsTestDoubles.Stub();

        int totalNumberOfResults = 11;

        SearchServiceResponse<FurtherEducationLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.Success, totalNumberOfResults)
            {
                LearnerSearchResults = stubResults.Results!,
                FacetedResults = stubResults.FacetResults!
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ReturnsAsync(serviceResponse);

        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _furtherEducationSearchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub(),
            FilterRequests = [FilterRequestTestDouble.Fake()],
            Offset = 3
        };

        FurtherEducationSearchByNameUseCase useCase =
            new FurtherEducationSearchByNameUseCase(mockService.Object);

        // Act
        SearchResponse<FurtherEducationLearners> response =
            await useCase.HandleRequestAsync(request);

        // Verify
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);

        // Assert (response mirrors service result)
        response.TotalNumberOfResults.Count.Should().Be(totalNumberOfResults);
        response.LearnerSearchResults!.Learners.Should().Contain(stubResults.Results!.Learners);
        response.FacetedResults!.Facets.Should().Contain(stubResults.FacetResults!.Facets);
    }
}
