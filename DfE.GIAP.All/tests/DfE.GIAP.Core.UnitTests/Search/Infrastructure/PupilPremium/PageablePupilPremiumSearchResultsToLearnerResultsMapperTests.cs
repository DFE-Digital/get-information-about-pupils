using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.PupilPremium;
public sealed class PageablePupilPremiumSearchResultsToLearnerResultsMapperTests
{
    private readonly
        IMapper<
            Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners> _searchResultsMapper;

    public PageablePupilPremiumSearchResultsToLearnerResultsMapperTests()
    {
        _searchResultsMapper =
            new PageablePupilPremiumSearchResultsToLearnerResultsMapper(
                new PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper()
        );
    }

    [Fact]
    public void Map_WithValidSearchResults_ReturnsConfiguredLearner()
    {
        // arrange
        List<SearchResult<PupilPremiumLearnerDataTransferObject>> searchResultDocuments =
            new SearchResultFakeBuilder<PupilPremiumLearnerDataTransferObject>()
                .WithSearchResults(PupilPremiumLearnerDataTransferObjectTestDoubles.Stub())
                .Create();

        Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>> pageableSearchResults =
            PageableTestDouble.FromResults(searchResultDocuments);

        // act
        PupilPremiumLearners? mappedResult =
            _searchResultsMapper.Map(pageableSearchResults);

        // assert
        mappedResult.Should().NotBeNull();
        mappedResult.Values.Should().HaveCount(searchResultDocuments.Count);

        foreach (SearchResult<PupilPremiumLearnerDataTransferObject> searchResult in searchResultDocuments)
        {
            PupilPremiumLearner? learner =
                mappedResult.Values.ToList()
                    .Find(learner =>
                        learner.Identifier.Value.ToString().Trim() == searchResult.Document.UPN);

            learner!.Name.FirstName
                .Should().Be(searchResult.Document.Forename);

            learner!.Name.MiddleNames
                .Should().Be(searchResult.Document.Middlenames);

            learner!.Name.Surname
                .Should().Be(searchResult.Document.Surname);

            (Convert.ToDateTime(learner.Characteristics.BirthDate))
                .Should().Be(searchResult.Document.DOB);

            learner.LocalAuthority.Code.ToString()
                .Should().Be(searchResult.Document.LocalAuthority);
        }
    }

    [Fact]
    public void Map_WithEmptySearchResults_ReturnsEmptyList()
    {
        // arrange
        List<SearchResult<PupilPremiumLearnerDataTransferObject>> emptySearchResultDocuments =
            new SearchResultFakeBuilder<PupilPremiumLearnerDataTransferObject>()
                .WithEmptySearchResult()
                .Create();

        // act
        PupilPremiumLearners? result =
            _searchResultsMapper.Map(
                PageableTestDouble.FromResults(emptySearchResultDocuments));

        // assert
        result.Should().NotBeNull();
        result.Values.Should().HaveCount(0);
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
        List<SearchResult<PupilPremiumLearnerDataTransferObject>> searchResultDocuments =
            new SearchResultFakeBuilder<PupilPremiumLearnerDataTransferObject>()
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
