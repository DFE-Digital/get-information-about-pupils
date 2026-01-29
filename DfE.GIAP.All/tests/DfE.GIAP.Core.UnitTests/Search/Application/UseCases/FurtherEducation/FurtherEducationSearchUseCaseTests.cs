using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.FurtherEducation;

public sealed class FurtherEducationSearchUseCaseTests
{
    private readonly SearchResults<FurtherEducationLearners, SearchFacets> _searchResults;
    private readonly ISearchCriteriaProvider _providerStub;
    private readonly SearchCriteria _furtherEducationSearchCriteriaStub;

    public FurtherEducationSearchUseCaseTests()
    {
        // arrange
        _searchResults = FurtherEducationSearchResultsTestDoubles.Stub();

        _furtherEducationSearchCriteriaStub = SearchCriteriaTestDouble.Stub();

        Mock<ISearchCriteriaProvider> provider = new();
        provider.Setup(mockProvider => mockProvider.GetCriteria(It.IsAny<string>())).Returns(_furtherEducationSearchCriteriaStub);
        _providerStub = provider.Object;
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_CallsAdapterWithMappedRequestParams()
    {
        // arrange
        Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> mockSearchServiceAdapter =
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

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
            new(_providerStub, mockSearchServiceAdapter.Object);

        // act
        FurtherEducationSearchResponse response =
            await useCase.HandleRequestAsync(request);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Once());

        // assert
        adapterRequest!.SearchKeyword.Should().Be(request.SearchKeywords);
        adapterRequest!.SearchFields.Should().BeEquivalentTo(_furtherEducationSearchCriteriaStub.SearchFields);
        adapterRequest!.Facets.Should().BeEquivalentTo(_furtherEducationSearchCriteriaStub.Facets);
        adapterRequest!.SearchFilterRequests.Should().BeEquivalentTo(request.FilterRequests);
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_ReturnsResponse()
    {
        // arrange
        Mock<ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>> mockSearchServiceAdapter =
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        FurtherEducationSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        FurtherEducationSearchUseCase useCase =
            new(_providerStub, mockSearchServiceAdapter.Object);

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
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        FurtherEducationSearchUseCase useCase =
            new(_providerStub, mockSearchServiceAdapter.Object);

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
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        FurtherEducationSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        Mock.Get(mockSearchServiceAdapter.Object)
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ThrowsAsync(new ApplicationException());

        FurtherEducationSearchUseCase useCase =
            new(_providerStub, mockSearchServiceAdapter.Object);

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
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        FurtherEducationSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        Mock.Get(mockSearchServiceAdapter.Object)
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ReturnsAsync(FurtherEducationSearchResultsTestDoubles.StubWithNoResults);

        FurtherEducationSearchUseCase useCase =
            new(_providerStub, mockSearchServiceAdapter.Object);

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
