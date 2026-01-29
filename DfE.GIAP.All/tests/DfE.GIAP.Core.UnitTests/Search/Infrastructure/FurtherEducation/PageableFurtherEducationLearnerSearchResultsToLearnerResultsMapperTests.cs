using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.FurtherEducation;

public sealed class PageableFurtherEducationLearnerSearchResultsToLearnerResultsMapperTests
{
    IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners> _searchResultsMapper;

    public PageableFurtherEducationLearnerSearchResultsToLearnerResultsMapperTests()
    {
        _searchResultsMapper =
            new PageableFurtherEducationSearchResultsToLearnerResultsMapper(
                new FurtherEducationSearchResultToLearnerMapper()
        );
    }

    [Fact]
    public void Map_WithValidSearchResults_ReturnsConfiguredLearner()
    {
        // arrange
        List<SearchResult<FurtherEducationLearnerDataTransferObject>> searchResultDocuments =
            new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                .WithSearchResults(FurtherEducationLearnerDataTransferObjectTestDouble.Fake())
                .Create();

        Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>> pageableSearchResults =
            PageableTestDouble.FromResults(searchResultDocuments);

        // act
        FurtherEducationLearners? mappedResult =
            _searchResultsMapper.Map(pageableSearchResults);

        // assert
        mappedResult.Should().NotBeNull();
        mappedResult.LearnerCollection.Should().HaveCount(searchResultDocuments.Count);

        foreach (SearchResult<FurtherEducationLearnerDataTransferObject> searchResult in searchResultDocuments)
        {
            FurtherEducationLearner? learner =
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
    public void Map_WithEmptySearchResults_ReturnsEmptyList()
    {
        // arrange
        List<SearchResult<FurtherEducationLearnerDataTransferObject>> emptySearchResultDocuments =
            new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                .WithEmptySearchResult()
                .Create();

        // act
        FurtherEducationLearners? result =
            _searchResultsMapper.Map(
                PageableTestDouble.FromResults(emptySearchResultDocuments));

        // assert
        result.Should().NotBeNull();
        result.LearnerCollection.Should().HaveCount(0);
    }

    [Fact]
    public void Map_WithNullSearchResults_ThrowsArgumentNullException()
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
    public void Map_WithANullSearchResult_ThrowsInvalidOperationException()
    {
        // arrange
        List<SearchResult<FurtherEducationLearnerDataTransferObject>> searchResultDocuments =
            new SearchResultFakeBuilder<FurtherEducationLearnerDataTransferObject>()
                .IncludeNullDocument()
                .Create();

        // act.
        _searchResultsMapper
            .Invoking(mapper =>
                mapper.Map(PageableTestDouble.FromResults(searchResultDocuments)))
                    .Should()
                        .Throw<InvalidOperationException>()
                        .WithMessage("Search result document object cannot be null.");
    }
}
