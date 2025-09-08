using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.Mappers;

public sealed class PageableSearchResultsToLearnerResultsMapperTests
{
    IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners> _searchResultsMapper;

    public PageableSearchResultsToLearnerResultsMapperTests()
    {
        _searchResultsMapper =
            new PageableSearchResultsToLearnerResultsMapper(
                new SearchResultToLearnerMapper()
        );
    }

    [Fact]
    public void MapFrom_WithValidSearchResults_ReturnsConfiguredEstablishments()
    {
        // arrange
        List<SearchResult<LearnerDataTransferObject>> searchResultDocuments =
            new SearchResultFakeBuilder().WithSearchResults().Create();

        Pageable<SearchResult<LearnerDataTransferObject>> pageableSearchResults =
            PageableTestDouble.FromResults(searchResultDocuments);

        // act
        Learners? mappedResult =
            _searchResultsMapper.Map(pageableSearchResults);

        // assert
        mappedResult.Should().NotBeNull();
        mappedResult.LearnerCollection.Should().HaveCount(searchResultDocuments.Count);

        foreach (SearchResult<LearnerDataTransferObject> searchResult in searchResultDocuments)
        {
            Learner? learner =
                mappedResult.LearnerCollection.ToList()
                    .Find(learner =>
                        learner.Identifier.UniqueLearnerNumber.ToString().Trim() == searchResult.Document.ULN);

            learner!.Name.FirstName
                .Should().Be(searchResult.Document.Forename);
            learner!.Name.Surname
                .Should().Be(searchResult.Document.Surname);
        }
    }

    [Fact]
    public void MapFrom_WithEmptySearchResults_ReturnsEmptyList()
    {
        // arrange
        List<SearchResult<LearnerDataTransferObject>> emptySearchResultDocuments =
            new SearchResultFakeBuilder().WithEmptySearchResult().Create();

        // act
        Learners? result =
            _searchResultsMapper.Map(
                PageableTestDouble.FromResults(emptySearchResultDocuments));

        // assert
        result.Should().NotBeNull();
        result.LearnerCollection.Should().HaveCount(0);
    }

    [Fact]
    public void MapFrom_WithNullSearchResults_ThrowsArgumentNullException()
    {
        // act.
        _searchResultsMapper
            .Invoking(mapper =>
                mapper.Map(null!))
                    .Should()
                        .Throw<ArgumentNullException>()
                        .WithMessage("Value cannot be null. (Parameter 'input')");
    }

    [Fact]
    public void MapFrom_WithANullSearchResult_ThrowsInvalidOperationException()
    {
        // arrange
        List<SearchResult<LearnerDataTransferObject>> searchResultDocuments =
            new SearchResultFakeBuilder().IncludeNullDocument().Create();

        // act.
        _searchResultsMapper
            .Invoking(mapper =>
                mapper.Map(PageableTestDouble.FromResults(searchResultDocuments)))
                    .Should()
                        .Throw<InvalidOperationException>()
                        .WithMessage("Search result document object cannot be null.");
    }
}
