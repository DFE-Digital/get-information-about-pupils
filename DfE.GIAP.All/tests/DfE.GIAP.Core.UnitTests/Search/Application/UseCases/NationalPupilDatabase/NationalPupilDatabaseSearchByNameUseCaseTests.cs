using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.NationalPupilDatabase;

public sealed class NationalPupilDatabaseSearchByNameUseCaseTests
{
    private readonly SearchCriteria _searchCriteriaStub;

    public NationalPupilDatabaseSearchByNameUseCaseTests()
    {
        // Arrange (shared stubs)
        _searchCriteriaStub = SearchCriteriaTestDouble.Stub();
    }

    [Fact]
    public void Constructor_Throws_When_Service_Is_Null()
    {
        // Arrange
        Func<NationalPupilDatabaseSearchByNameUseCase> construct =
            () => new NationalPupilDatabaseSearchByNameUseCase(searchForLearnerByNameService: null!);

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(construct);
    }


    [Fact]
    public async Task HandleRequest_NullRequest_Throws()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<NationalPupilDatabaseLearners>> mockService = new();

        NationalPupilDatabaseSearchByNameUseCase useCase = new(mockService.Object);

        // Act
        Func<Task> act = async () => _ = await useCase.HandleRequestAsync(request: null!);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task HandleRequest_ServiceThrows_ExceptionBubbles()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<NationalPupilDatabaseLearners>> mockService =
            new();

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ThrowsAsync(new ApplicationException("service failure"));

        NationalPupilDatabaseSearchByNameUseCase useCase =
            new(mockService.Object);

        NationalPupilDatabaseSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _searchCriteriaStub,
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
        Mock<ISearchLearnerByNameService<NationalPupilDatabaseLearners>> mockService = new();

        ISearchServiceAdaptorResponse<NationalPupilDatabaseLearners, SearchFacets> noResults =
            NationalPupilDatabaseSearchResultsTestDoubles.StubWithNoResults();

        SearchServiceResponse<NationalPupilDatabaseLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.NoResultsFound, 0)
            {
                LearnerSearchResults = noResults.Results!,
                FacetedResults = noResults.FacetResults
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ReturnsAsync(serviceResponse);

        NationalPupilDatabaseSearchByNameUseCase useCase = new(mockService.Object);

        NationalPupilDatabaseSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _searchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub()
        };

        // Act
        SearchResponse<NationalPupilDatabaseLearners> response = await useCase.HandleRequestAsync(request);

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
        Mock<ISearchLearnerByNameService<NationalPupilDatabaseLearners>> mockService = new();

        SearchLearnerByNameRequest? captured = null;

        // Return a minimal successful response from the service
        NationalPupilDatabaseLearners learners = NationalPupilDatabaseSearchResultsTestDoubles.Stub().Results!;
        SearchFacets facets = NationalPupilDatabaseSearchResultsTestDoubles.Stub().FacetResults!;
        int totalNumberOfResults = 42;

        SearchServiceResponse<NationalPupilDatabaseLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.Success, totalNumberOfResults)
            {
                LearnerSearchResults = learners,
                FacetedResults = facets
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .Callback<SearchLearnerByNameRequest>(req => captured = req)
            .ReturnsAsync(serviceResponse);

        NationalPupilDatabaseSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            FilterRequests = [FilterRequestTestDouble.Fake()],
            SearchCriteria = _searchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub(),
            Offset = 7
        };

        NationalPupilDatabaseSearchByNameUseCase useCase = new(mockService.Object);

        // Act
        SearchResponse<NationalPupilDatabaseLearners> response = await useCase.HandleRequestAsync(request);

        // Verify
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);

        // Assert (mapping into service request)
        captured.Should().NotBeNull();
        captured!.SearchKeywords.Should().Be(request.SearchKeywords);
        captured.SearchCriteria.Should().BeSameAs(request.SearchCriteria);
        captured.SortOrder.Should().Be(request.SortOrder);
        captured.FilterRequests.Should().BeEquivalentTo(request.FilterRequests);
        captured.Offset.Should().Be(request.Offset);

        // Assert (returned response reflects service response)
        response.LearnerSearchResults.Should().BeSameAs(learners);
        response.FacetedResults.Should().BeSameAs(facets);
        response.TotalNumberOfResults.Count.Should().Be(totalNumberOfResults);
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_ReturnsResponse_From_Service()
    {
        // Arrange
        Mock<ISearchLearnerByNameService<NationalPupilDatabaseLearners>> mockService = new();

        // Use the shared doubles to produce consistent structures for assertions
        ISearchServiceAdaptorResponse<NationalPupilDatabaseLearners, SearchFacets> stubResults =
            NationalPupilDatabaseSearchResultsTestDoubles.Stub();

        int totalNumberOfResults = 11;

        SearchServiceResponse<NationalPupilDatabaseLearners, SearchFacets> serviceResponse =
            new(SearchResponseStatus.Success, totalNumberOfResults)
            {
                LearnerSearchResults = stubResults.Results!,
                FacetedResults = stubResults.FacetResults!
            };

        mockService
            .Setup(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()))
            .ReturnsAsync(serviceResponse);

        NationalPupilDatabaseSearchByNameRequest request = new()
        {
            SearchKeywords = "searchkeyword",
            SearchCriteria = _searchCriteriaStub,
            SortOrder = SortOrderTestDouble.Stub(),
            FilterRequests = [FilterRequestTestDouble.Fake()],
            Offset = 3
        };

        NationalPupilDatabaseSearchByNameUseCase useCase = new(mockService.Object);

        // Act
        SearchResponse<NationalPupilDatabaseLearners> response = await useCase.HandleRequestAsync(request);

        // Verify
        mockService.Verify(s => s.SearchAsync(It.IsAny<SearchLearnerByNameRequest>()), Times.Once);

        // Assert (response mirrors service result)
        response.TotalNumberOfResults.Count.Should().Be(totalNumberOfResults);
        response.LearnerSearchResults!.Values.Should().Contain(stubResults.Results!.Values);
        response.FacetedResults!.Facets.Should().Contain(stubResults.FacetResults!.Facets);
    }
}
