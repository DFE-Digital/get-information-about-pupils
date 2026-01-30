using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using FluentAssertions;
namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.PupilPremium;


public sealed class PupilPremiumSearchUseCaseTests
{
    private readonly SearchResults<PupilPremiumLearners, SearchFacets> _searchResults;
    private readonly ISearchCriteriaProvider _providerStub;
    private readonly SearchCriteria _searchCriteriaStub;

    public PupilPremiumSearchUseCaseTests()
    {
        // arrange (shared stubs)
        _searchResults = PupilPremiumSearchResultsTestDoubles.Stub();

        _searchCriteriaStub = SearchCriteriaTestDouble.Stub();

        Mock<ISearchCriteriaProvider> provider = new();
        provider.Setup(mockProvider => mockProvider.GetCriteria(It.IsAny<string>())).Returns(_searchCriteriaStub);
        _providerStub = provider.Object;
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_CallsAdapterWithMappedRequestParams()
    {
        // arrange
        Mock<ISearchServiceAdapter<PupilPremiumLearners, SearchFacets>> mockSearchServiceAdapter =
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        SearchServiceAdapterRequest? adapterRequest = null;
        Mock.Get(mockSearchServiceAdapter.Object)
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .Callback<SearchServiceAdapterRequest>((input) =>
            {
                adapterRequest = input;
            })
            .ReturnsAsync(_searchResults);

        PupilPremiumSearchRequest request =
            new(
                searchKeywords: "searchkeyword",
                filterRequests: [FilterRequestTestDouble.Fake()],
                sortOrder: SortOrderTestDouble.Stub()
            );

        PupilPremiumSearchUseCase useCase = new(_providerStub, mockSearchServiceAdapter.Object);

        // act
        PupilPremiumSearchResponse response =
            await useCase.HandleRequestAsync(request);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Once());

        // assert
        adapterRequest!.SearchKeyword.Should().Be(request.SearchKeywords);
        adapterRequest!.SearchFields.Should().BeEquivalentTo(_searchCriteriaStub.SearchFields);
        adapterRequest!.Facets.Should().BeEquivalentTo(_searchCriteriaStub.Facets);
        adapterRequest!.SearchFilterRequests.Should().BeEquivalentTo(request.FilterRequests);
    }

    [Fact]
    public async Task HandleRequest_ValidRequest_ReturnsResponse()
    {
        // arrange
        Mock<ISearchServiceAdapter<PupilPremiumLearners, SearchFacets>> mockSearchServiceAdapter =
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        PupilPremiumSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        PupilPremiumSearchUseCase useCase = new(_providerStub, mockSearchServiceAdapter.Object);

        // act
        PupilPremiumSearchResponse response =
            await useCase.HandleRequestAsync(request);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Once());

        // assert
        response.Status.Should().Be(SearchResponseStatus.Success);
        response.LearnerSearchResults!.Values.Should().Contain(_searchResults.Results!.Values);
        response.FacetedResults!.Facets.Should().Contain(_searchResults.FacetResults!.Facets);
    }

    [Fact]
    public async Task HandleRequest_NullSearchByKeywordRequest_ReturnsErrorStatus()
    {
        // arrange
        Mock<ISearchServiceAdapter<PupilPremiumLearners, SearchFacets>> mockSearchServiceAdapter =
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        PupilPremiumSearchUseCase useCase =
            new(_providerStub, mockSearchServiceAdapter.Object);

        // act
        PupilPremiumSearchResponse response =
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
        Mock<ISearchServiceAdapter<PupilPremiumLearners, SearchFacets>> mockSearchServiceAdapter =
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        PupilPremiumSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        Mock.Get(mockSearchServiceAdapter.Object)
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ThrowsAsync(new ApplicationException());

        PupilPremiumSearchUseCase useCase =
            new(_providerStub, mockSearchServiceAdapter.Object);

        // act
        PupilPremiumSearchResponse response =
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
        Mock<ISearchServiceAdapter<PupilPremiumLearners, SearchFacets>> mockSearchServiceAdapter =
            SearchServiceAdapterTestDouble.MockFor(_searchResults);

        PupilPremiumSearchRequest request = new(searchKeywords: "searchkeyword", sortOrder: SortOrderTestDouble.Stub());

        Mock.Get(mockSearchServiceAdapter.Object)
            .Setup(adapter => adapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()))
            .ReturnsAsync(PupilPremiumSearchResultsTestDoubles.StubWithNoResults());

        PupilPremiumSearchUseCase useCase =
            new(_providerStub, mockSearchServiceAdapter.Object);

        // act
        PupilPremiumSearchResponse response =
            await useCase.HandleRequestAsync(request);

        // verify
        mockSearchServiceAdapter.Verify(searchServiceAdapter =>
            searchServiceAdapter.SearchAsync(It.IsAny<SearchServiceAdapterRequest>()), Times.Once());

        // assert
        response.Status
            .Should().Be(SearchResponseStatus.NoResultsFound);
    }
}
