using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Request;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Response;
using DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases;

public sealed class SearchByKeyWordsUseCaseTests
{
    private readonly SearchResults<FurtherEducationLearners, SearchFacets> _searchResults;
    private readonly SearchCriteria _searchByKeywordCriteriaStub = SearchCriteriaTestDouble.Stub();

    public SearchByKeyWordsUseCaseTests()
    {
        // arrange
        _searchResults = SearchResultsTestDouble.Stub();
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_CallsAdapterWithMappedRequestParams()
    {
        // arrange
        Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> mockSearchServiceAdapter =
            new SearchServiceAdapterTestDouble().MockFor(_searchResults);

        SearchServiceAdapterRequest? adapterRequest = null;
        Mock.Get(mockSearchServiceAdapter.Object)
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .Callback<SearchServiceAdapterRequest>((input) =>
            {
                adapterRequest = input;
            });

        FurtherEducationSearchRequest request =
            new(
                searchKeywords: "searchkeyword",
                filterRequests: [FilterRequestTestDouble.Fake()],
                sortOrder: SortOrderTestDouble.Stub()
            );

        FurtherEducationSearchUseCase useCase =
            new(_searchByKeywordCriteriaStub, mockSearchServiceAdapter.Object);

        // act
        FurtherEducationSearchResponse response =
            await useCase.HandleRequestAsync(request);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Once());

        // assert
        adapterRequest!.SearchKeyword.Should().Be(request.SearchKeywords);
        adapterRequest!.SearchFields.Should().BeEquivalentTo(_searchByKeywordCriteriaStub.SearchFields);
        adapterRequest!.Facets.Should().BeEquivalentTo(_searchByKeywordCriteriaStub.Facets);
        adapterRequest!.SearchFilterRequests.Should().BeEquivalentTo(request.FilterRequests);
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_ReturnsResponse()
    {
        // arrange
        Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> mockSearchServiceAdapter =
            new SearchServiceAdapterTestDouble().MockFor(_searchResults);

        FurtherEducationSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        FurtherEducationSearchUseCase useCase =
            new(_searchByKeywordCriteriaStub, mockSearchServiceAdapter.Object);

        // act
        FurtherEducationSearchResponse response =
            await useCase.HandleRequestAsync(request);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Once());

        // assert
        response.Status.Should().Be(SearchResponseStatus.Success);
        response.LearnerSearchResults!.LearnerCollection.Should().Contain(_searchResults.Results!.LearnerCollection);
        response.FacetedResults!.Facets.Should().Contain(_searchResults.FacetResults!.Facets);
    }

    [Fact]
    public async Task HandleRequest_NullSearchByKeywordRequest_ReturnsErrorStatus()
    {
        // arrange
        Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> mockSearchServiceAdapter =
            new SearchServiceAdapterTestDouble().MockFor(_searchResults);

        FurtherEducationSearchUseCase useCase =
            new(_searchByKeywordCriteriaStub, mockSearchServiceAdapter.Object);

        // act
        FurtherEducationSearchResponse response =
            await useCase.HandleRequestAsync(request: null!);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Never());

        // assert
        response.Status.Should()
                .Be(SearchResponseStatus.InvalidRequest);
    }

    [Fact]
    public async Task HandleRequest_ServiceAdapterThrowsException_ReturnsErrorStatus()
    {
        // arrange
        Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> mockSearchServiceAdapter =
            new SearchServiceAdapterTestDouble().MockFor(_searchResults);

        FurtherEducationSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        Mock.Get(mockSearchServiceAdapter.Object)
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ThrowsAsync(new ApplicationException());

        FurtherEducationSearchUseCase useCase =
            new(_searchByKeywordCriteriaStub, mockSearchServiceAdapter.Object);

        // act
        FurtherEducationSearchResponse response =
            await useCase.HandleRequestAsync(request);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Once());

        // assert
        response.Status.Should()
                .Be(SearchResponseStatus.SearchServiceError);
    }

    [Fact]
    public async Task HandleRequest_NoResults_ReturnsSuccess()
    {
        // arrange
        Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> mockSearchServiceAdapter =
            new SearchServiceAdapterTestDouble().MockFor(_searchResults);

        FurtherEducationSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        Mock.Get(mockSearchServiceAdapter.Object)
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ReturnsAsync(SearchResultsTestDouble.StubWithNoResults);

        FurtherEducationSearchUseCase useCase =
            new(_searchByKeywordCriteriaStub, mockSearchServiceAdapter.Object);

        // act
        FurtherEducationSearchResponse response =
            await useCase.HandleRequestAsync(request);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Once());

        // assert
        response.Status
            .Should().Be(SearchResponseStatus.NoResultsFound);
    }
}
