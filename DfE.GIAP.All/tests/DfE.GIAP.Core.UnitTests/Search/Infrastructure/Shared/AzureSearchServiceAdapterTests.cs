using Azure;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Shared;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Builders;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
using DfE.GIAP.Web.Features.Search.Options.Search;
using FluentAssertions;
using Microsoft.Extensions.Options;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.Shared;

public sealed class AzureSearchServiceAdaptorTests
{
    private readonly string _mockIndexKey = "further-education";

    private readonly SearchOptions _searchOptions = SearchOptionsTestDouble.Stub();

    private readonly Mock<IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets>> _mockFacetsMapper =
        FacetResultToLearnerFacetsMapperTestDouble.DefaultMock();

    private readonly ISearchOptionsBuilder _searchOptionsBuilder =
        new SearchOptionsBuilder(FilterExpressionTestDouble.Mock());

    private SearchIndexOptions IndexOptions =>
        _searchOptions.Indexes![_mockIndexKey];

    private static IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>
        CreateDtoToResultMapper() =>
            MapperTestDoubles
                .MockFor<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>,
                          FurtherEducationLearners>()
                .Object;

    [Fact]
    public void Ctor_Throws_When_SearchByKeywordService_Is_Null()
    {
        // Arrange
        ISearchByKeywordService nullService = null!;
        IOptions<SearchOptions> options = OptionsTestDoubles.MockAs(_searchOptions);

        // Act
        Func<AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>> act =
            () => new AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>(
                nullService,
                _mockFacetsMapper.Object,
                _searchOptionsBuilder,
                CreateDtoToResultMapper());

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Ctor_Throws_When_DtoToResultMapper_Is_Null()
    {
        // Arrange
        ISearchByKeywordService service = SearchServiceTestDouble.DefaultMock();
        IOptions<SearchOptions> options = OptionsTestDoubles.MockAs(_searchOptions);
        IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners> nullMapper = null!;

        // Act
        Func<AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>> act =
            () => new AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>(
                service,
                _mockFacetsMapper.Object,
                _searchOptionsBuilder,
                nullMapper);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public async Task SearchAsync_Forwards_Correct_Parameters_To_ISearchByKeywordService()
    {
        // Arrange
        Mock<Response> responseMock = new();

        FurtherEducationLearnerDataTransferObject fakeLearner =
            FurtherEducationLearnerDataTransferObjectTestDouble.Fake();

        Response<SearchResults<FurtherEducationLearnerDataTransferObject>> searchResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                        .WithSearchResults(fakeLearner)
                        .Create(),
                    IndexOptions.SearchCriteria!.Size,
                    facets: null,
                    coverage: null,
                    rawResponse: responseMock.Object),
                responseMock.Object);

        Mock<ISearchByKeywordService> mockKeywordService =
            SearchByKeywordServiceTestDouble.MockFor(searchResponse);

        IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners> dtoMapper = CreateDtoToResultMapper();

        AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject> sut =
            new(
                mockKeywordService.Object,
                _mockFacetsMapper.Object,
                _searchOptionsBuilder,
                dtoMapper);

        SearchServiceAdapterRequest request =
            SearchServiceAdapterRequestTestDouble.Stub(_mockIndexKey);

        // Act
        await sut.SearchAsync(request);

        SearchByKeywordServiceTestDouble.keywordPassedToSearchService.Should().Be(request.SearchKeyword);
        SearchByKeywordServiceTestDouble.indexPassedToSearchService.Should().Be(request.Index);

        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.Size.Should().Be(request.Size);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.SearchMode.Should()
            .Be((SearchMode)IndexOptions.SearchCriteria!.SearchMode);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.IncludeTotalCount.Should()
            .Be(request.IncludeTotalCount);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.SearchFields.Should()
            .BeEquivalentTo(request.SearchFields);
        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.Facets.Should()
            .BeEquivalentTo(request.Facets);
    }

    [Fact]
    public async Task SearchAsync_Maps_Facets_When_Present()
    {
        // Arrange
        Mock<Response> responseMock = new();
        _mockFacetsMapper
            .Setup(m => m.Map(It.IsAny<Dictionary<string, IList<AzureFacetResult>>>()))
            .Returns(SearchFacetsTestDouble.Stub());


        IDictionary<string, IList<AzureFacetResult>> rawFacets =
            new Dictionary<string, IList<AzureFacetResult>>
            {
            {
                "facet1",
                new List<AzureFacetResult>
                {
                    SearchModelFactory.FacetResult(
                        count: 3,
                        additionalProperties: new Dictionary<string, object> { ["value"] = "A" })
                }
            }
            };
        Response<SearchResults<FurtherEducationLearnerDataTransferObject>> searchResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                        .WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake())
                        .Create(),
                    1,
                    facets: rawFacets,
                    coverage: null,
                    rawResponse: responseMock.Object),
                responseMock.Object);

        Mock<ISearchByKeywordService> mockKeywordService =
            SearchByKeywordServiceTestDouble.MockFor(searchResponse);

        AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject> sut =
            new AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>(
                mockKeywordService.Object,
                _mockFacetsMapper.Object,
                _searchOptionsBuilder,
                CreateDtoToResultMapper());

        SearchServiceAdapterRequest request =
            SearchServiceAdapterRequestTestDouble.Stub(_mockIndexKey);

        // Act
        ISearchResults<FurtherEducationLearners, SearchFacets> result =
            await sut.SearchAsync(request);

        // Assert
        result.FacetResults.Should().NotBeNull();
        _mockFacetsMapper.Verify(
            m => m.Map(It.IsAny<Dictionary<string, IList<AzureFacetResult>>>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_Sets_Facets_To_Null_When_Absent()
    {
        // Arrange
        Mock<Response> responseMock = new();

        Response<SearchResults<FurtherEducationLearnerDataTransferObject>> searchResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                        .WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake())
                        .Create(),
                    1,
                    facets: null,
                    coverage: null,
                    rawResponse: responseMock.Object),
                responseMock.Object);

        Mock<ISearchByKeywordService> mockKeywordService =
            SearchByKeywordServiceTestDouble.MockFor(searchResponse);

        AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject> sut =
            new AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject>(
                mockKeywordService.Object,
                _mockFacetsMapper.Object,
                _searchOptionsBuilder,
                CreateDtoToResultMapper());

        SearchServiceAdapterRequest request =
            SearchServiceAdapterRequestTestDouble.Stub(_mockIndexKey);

        // Act
        ISearchResults<FurtherEducationLearners, SearchFacets> result =
            await sut.SearchAsync(request);

        // Assert
        result.FacetResults.Should().BeNull();
    }

    [Fact]
    public Task SearchAsync_When_DtoMapper_Throws_It_Propagates()
    {
        // Arrange
        Mock<IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>> throwingMapper =
            MapperTestDoubles.MockFor<
                Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>,
                FurtherEducationLearners>();

        throwingMapper
            .Setup(m => m.Map(It.IsAny<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>>()))
            .Throws(new ArgumentException("bad map"));

        Mock<Response> responseMock = new();

        Response<SearchResults<FurtherEducationLearnerDataTransferObject>> searchResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                        .WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake())
                        .Create(),
                    1,
                    facets: null,
                    coverage: null,
                    rawResponse: responseMock.Object),
                responseMock.Object);

        Mock<ISearchByKeywordService> mockKeywordService =
            SearchByKeywordServiceTestDouble.MockFor(searchResponse);

        AzureSearchServiceAdaptor<FurtherEducationLearners, FurtherEducationLearnerDataTransferObject> sut =
            new(
                mockKeywordService.Object,
                _mockFacetsMapper.Object,
                _searchOptionsBuilder,
                throwingMapper.Object);

        SearchServiceAdapterRequest request =
            SearchServiceAdapterRequestTestDouble.Stub(_mockIndexKey);

        // Act & Assert
        return sut.Invoking(s => s.SearchAsync(request))
            .Should()
            .ThrowAsync<ArgumentException>();
    }
}
