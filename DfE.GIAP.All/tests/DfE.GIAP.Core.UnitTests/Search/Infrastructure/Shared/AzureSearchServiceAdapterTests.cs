using Azure;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Shared;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Builders;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.Options;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;
using FacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.Shared;

public sealed class AzureSearchByKeywordServiceTests
{
    private readonly string _mockSearchIndexKey = "further-education";

    private readonly Mock<IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets>> _mockFacetsMapper =
        FacetResultToLearnerFacetsMapperTestDouble.DefaultMock();

    private readonly AzureSearchOptions _mockAzureSearchOptions = AzureSearchOptionsTestDouble.Stub();

    private readonly ISearchOptionsBuilder _mockSearchOptionsBuilder =
        new SearchOptionsBuilder(searchFilterExpressionsBuilder: FilterExpressionTestDouble.Mock());

    private AzureSearchIndexOptions _mockAzureIndexOptions =>
        _mockAzureSearchOptions.Indexes![_mockSearchIndexKey];

    private static IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>
        CreateMapper() =>
            MapperTestDoubles.MockFor<
                Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>().Object;

    [Fact]
    public void Ctor_Throws_When_AzureSearchOptions_Is_Null()
    {
        // Arrange
        ISearchByKeywordService searchByKeywordService = SearchServiceTestDouble.DefaultMock();
        IOptions<AzureSearchOptions> nullOptions = OptionsTestDoubles.MockNullOptions<AzureSearchOptions>();
        ISearchOptionsBuilder searchOptionsBuilder = _mockSearchOptionsBuilder;

        // Act
        Func<AzureSearchByKeywordService> act = () =>
            new(
                searchByKeywordService,
                nullOptions,
                _mockFacetsMapper.Object,
                searchOptionsBuilder);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }


    [Fact]
    public async Task SearchByKeywordAsync_Sends_Correct_Request_To_ISearchByKeywordService()
    {
        // Arrange
        Mock<Response> responseMock = new();
        Response<SearchResults<FurtherEducationLearnerDataTransferObject>> searchServiceResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                        .WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake())
                        .Create(),
                    10, null, null, responseMock.Object),
                responseMock.Object);

        Mock<ISearchByKeywordService> mockSearchByKeywordService =
            SearchByKeywordServiceTestDouble.MockFor(searchServiceResponse);

        IOptions<AzureSearchOptions> options = OptionsTestDoubles.MockAs(_mockAzureSearchOptions);



        AzureSearchByKeywordService sut =
            new(
                mockSearchByKeywordService.Object,
                options,
                _mockFacetsMapper.Object,
                _mockSearchOptionsBuilder);

        SearchServiceAdapterRequest request = SearchServiceAdapterRequestTestDouble.Stub(searchIndexKey: _mockSearchIndexKey);

        IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners> mapper =
            CreateMapper();

        // Act
        _ = await sut.SearchByKeywordAsync(request, mapper);

        // Assert
        SearchByKeywordServiceTestDouble.keywordPassedToSearchService
            .Should().Be(request.SearchKeyword);

        SearchByKeywordServiceTestDouble.indexPassedToSearchService
            .Should().Be(_mockAzureIndexOptions.SearchIndex);

        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.Size
            .Should().Be(_mockAzureIndexOptions.Size);

        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.SearchMode
            .Should().Be((SearchMode)_mockAzureIndexOptions.SearchMode);

        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.IncludeTotalCount
            .Should().Be(_mockAzureIndexOptions.IncludeTotalCount);

        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.SearchFields
            .Should().BeEquivalentTo(request.SearchFields);

        SearchByKeywordServiceTestDouble.searchOptionsPassedToSearchService!.Facets
            .Should().BeEquivalentTo(request.Facets);
    }

    [Fact]
    public async Task SearchByKeywordAsync_Maps_Facets_When_Present()
    {
        // Arrange: craft a response that includes facets
        Mock<Response> responseMock = new();

        // Build a small fake SearchResults with a Facets dictionary

        IDictionary<string, IList<FacetResult>> sdkFacets =
            new Dictionary<string, IList<FacetResult>>
            {
                {
                    "facet1",
                    new List<FacetResult>
                    {
                        // bucket: value="Value1", count=5
                        SearchModelFactory.FacetResult(
                            count: 5,
                            additionalProperties: new Dictionary<string, object>
                            {
                                ["value"] = "Value1"
                            }),
                    }
                }
            };

        _mockFacetsMapper.Setup(t => t.Map(It.IsAny<Dictionary<string, IList<AzureFacetResult>>>()))
            .Returns(SearchFacetsTestDouble.Stub());

        Response<SearchResults<FurtherEducationLearnerDataTransferObject>> searchServiceResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                        .WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake())
                        .Create(),
                    10,
                    facets: sdkFacets,
                    coverage: null,
                    rawResponse: responseMock.Object),
                responseMock.Object);

        Mock<ISearchByKeywordService> mockSearchByKeywordService =
            SearchByKeywordServiceTestDouble.MockFor(searchServiceResponse);

        IOptions<AzureSearchOptions> options = OptionsTestDoubles.MockAs(_mockAzureSearchOptions);

        AzureSearchByKeywordService sut =
            new AzureSearchByKeywordService(
                mockSearchByKeywordService.Object,
                options,
                _mockFacetsMapper.Object,
                _mockSearchOptionsBuilder);

        SearchServiceAdapterRequest request = SearchServiceAdapterRequestTestDouble.Stub(searchIndexKey: _mockSearchIndexKey);

        IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners> mapper =
            CreateMapper();

        // Act
        SearchResults<FurtherEducationLearners, SearchFacets> result =
            await sut.SearchByKeywordAsync(request, mapper);

        // Assert
        result.Should().NotBeNull();
        result.FacetResults.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchByKeywordAsync_Sets_Facets_To_Null_When_Absent()
    {
        // Arrange: response with no facets (null)
        Mock<Response> responseMock = new();

        Response<SearchResults<FurtherEducationLearnerDataTransferObject>> searchServiceResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>().WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake()).Create(),
                    10,
                    facets: null,
                    coverage: null,
                    rawResponse: responseMock.Object),
                responseMock.Object);

        Mock<ISearchByKeywordService> mockSearchByKeywordService =
            SearchByKeywordServiceTestDouble.MockFor(searchServiceResponse);

        IOptions<AzureSearchOptions> options = OptionsTestDoubles.MockAs(_mockAzureSearchOptions);

        AzureSearchByKeywordService sut = new(
                mockSearchByKeywordService.Object,
                options,
                _mockFacetsMapper.Object,
                _mockSearchOptionsBuilder);

        SearchServiceAdapterRequest request = SearchServiceAdapterRequestTestDouble.Stub(searchIndexKey: _mockSearchIndexKey);

        IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners> mapper =
            CreateMapper();

        // Act
        SearchResults<FurtherEducationLearners, SearchFacets> result =
            await sut.SearchByKeywordAsync(request, mapper);

        // Assert
        result.Should().NotBeNull();
        result.FacetResults.Should().BeNull();
    }

    [Fact]
    public Task SearchByKeywordAsync_When_Mapper_Throws_Exception_It_Propagates()
    {
        // Arrange: mapper throws when Map is called
        Mock<IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>> throwingMapper =
            MapperTestDoubles.MockFor<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>();

        throwingMapper
            .Setup(m => m.Map(It.IsAny<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>>()))
            .Throws(new ArgumentException("bad mapping"));

        Mock<Response> responseMock = new();

        Response<SearchResults<FurtherEducationLearnerDataTransferObject>> searchServiceResponse =
            Response.FromValue(
                SearchModelFactory.SearchResults(
                    new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                        .WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake())
                        .Create(),
                    10,
                    null,
                    null,
                    responseMock.Object),
                responseMock.Object);

        Mock<ISearchByKeywordService> mockSearchByKeywordService =
            SearchByKeywordServiceTestDouble.MockFor(searchServiceResponse);

        IOptions<AzureSearchOptions> options = OptionsTestDoubles.MockAs(_mockAzureSearchOptions);

        AzureSearchByKeywordService sut =
            new(
                mockSearchByKeywordService.Object,
                options,
                _mockFacetsMapper.Object,
                _mockSearchOptionsBuilder);

        SearchServiceAdapterRequest request = SearchServiceAdapterRequestTestDouble.Stub(searchIndexKey: _mockSearchIndexKey);

        // Act & Assert
        return sut
            .Invoking(s => s.SearchByKeywordAsync(request, throwingMapper.Object))
            .Should()
            .ThrowAsync<ArgumentException>();
    }
}
