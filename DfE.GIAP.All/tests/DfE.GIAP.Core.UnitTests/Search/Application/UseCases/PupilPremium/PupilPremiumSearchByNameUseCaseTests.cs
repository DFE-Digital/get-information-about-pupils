using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByName;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using FluentAssertions;
namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.PupilPremium;

public sealed class PupilPremiumSearchByNameUseCaseTests
{
    private readonly SearchCriteria _searchCriteriaStub;

    public PupilPremiumSearchByNameUseCaseTests()
    {
        // Arrange (shared stubs)
        _searchCriteriaStub = SearchCriteriaTestDouble.Stub();
    }

    [Fact]
    public void Constructor_Throws_When_Service_Is_Null()
    {
        // Arrange
        Func<PupilPremiumSearchByNameUseCase> construct =
            () => new(searchForLearnerByNameService: null!);

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequest_NullRequest_Throws()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<PupilPremiumLearners>> mockService = new();

        PupilPremiumSearchByNameUseCase useCase = new(mockService.Object);

        // Act
        Func<Task> act = async () => _ = await useCase.HandleRequestAsync(request: null!);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequest_ServiceThrows_ExceptionBubbles()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<PupilPremiumLearners>> mockService = new();

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ThrowsAsync(new ApplicationException("service failure"));

        PupilPremiumSearchByNameUseCase useCase =
            new(mockService.Object);

        PupilPremiumSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _searchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub()
        };

        // Act
        Func<Task> act = async () => await useCase.HandleRequestAsync(request);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>();
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleRequest_NoResults_Still_Returns_Response_With_Empty_Collections_And_Total()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<PupilPremiumLearners>> mockService = new();

        ISearchServiceAdaptorResponse<PupilPremiumLearners, SearchFacets> noResults =
            PupilPremiumSearchResultsTestDoubles.StubWithNoResults();

        SearchServiceResponse<PupilPremiumLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.NoResultsFound, 0)
            {
                LearnerSearchResults = noResults.Results!,
                FacetedResults = noResults.FacetResults
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ReturnsAsync(serviceResponse);

        PupilPremiumSearchByNameUseCase useCase =
            new(mockService.Object);

        PupilPremiumSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _searchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub()
        };

        // Act
        SearchResponse<PupilPremiumLearners> response = await useCase.HandleRequestAsync(request);

        // Verify
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);

        // Assert
        response.TotalNumberOfResults.Count.Should().Be(0);
        response.LearnerSearchResults!.Values.Should().BeEmpty();
        response.FacetedResults.Facets.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_CallsServiceWithMappedRequestParams()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<PupilPremiumLearners>> mockService = new();

        SearchLearnerByNameRequest? captured = null;

        // Return a minimal successful response from the service
        ISearchServiceAdaptorResponse<PupilPremiumLearners, SearchFacets> stub = PupilPremiumSearchResultsTestDoubles.Stub();
        PupilPremiumLearners learners = stub.Results!;
        SearchFacets facets = stub.FacetResults!;
        int totalNumberOfResults = 42;

        SearchServiceResponse<PupilPremiumLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.Success, totalNumberOfResults)
            {
                LearnerSearchResults = learners,
                FacetedResults = facets
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .Callback<SearchLearnerByNameRequest>(req => captured = req)
            .ReturnsAsync(serviceResponse);

        PupilPremiumSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _searchCriteriaStub,
            FilterRequests = [FilterRequestTestDouble.Fake()],
            SortOrder = SortOrderTestDouble.Stub(),
            Offset = 7
        };

        PupilPremiumSearchByNameUseCase useCase = new(mockService.Object);

        // Act
        SearchResponse<PupilPremiumLearners> response = await useCase.HandleRequestAsync(request);

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
        Mock<ISearchLearnerByNameService<PupilPremiumLearners>> mockService = new();

        // Use the shared doubles to produce consistent structures for assertions
        ISearchServiceAdaptorResponse<PupilPremiumLearners, SearchFacets> stubResults =
            PupilPremiumSearchResultsTestDoubles.Stub();

        int totalNumberOfResults = 11;

        SearchServiceResponse<PupilPremiumLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.Success, totalNumberOfResults)
            {
                LearnerSearchResults = stubResults.Results!,
                FacetedResults = stubResults.FacetResults!
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ReturnsAsync(serviceResponse);

        PupilPremiumSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _searchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub(),
            FilterRequests = [FilterRequestTestDouble.Fake()],
            Offset = 3
        };

        PupilPremiumSearchByNameUseCase useCase = new(mockService.Object);

        // Act
        SearchResponse<PupilPremiumLearners> response = await useCase.HandleRequestAsync(request);

        // Verify
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);

        // Assert (response mirrors service result)
        response.TotalNumberOfResults.Count.Should().Be(totalNumberOfResults);
        response.LearnerSearchResults!.Values.Should().Contain(stubResults.Results!.Values);
        response.FacetedResults!.Facets.Should().Contain(stubResults.FacetResults!.Facets);
    }
}
