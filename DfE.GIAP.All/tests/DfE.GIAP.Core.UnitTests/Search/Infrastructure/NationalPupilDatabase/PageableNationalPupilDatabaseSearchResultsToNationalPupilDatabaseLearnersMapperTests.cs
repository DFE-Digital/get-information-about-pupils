using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.NationalPupilDatabase;
public sealed class PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapperTests
{
    private readonly PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapper _sut;

    public PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapperTests()
    {
        _sut =
            new(
                new NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper()
                );
    }

    [Fact]
    public void Map_WithValidSearchResults_ReturnsConfiguredLearner()
    {
        // arrange
        List<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>> searchResultDocuments =
            new SearchResultFakeBuilder<NationalPupilDatabaseLearnerDataTransferObject>()
                .WithSearchResults(NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub())
                .Create();

        Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>> pageableSearchResults =
            PageableTestDouble.FromResults(searchResultDocuments);

        // act
        NationalPupilDatabaseLearners? mappedResult =
            _sut.Map(pageableSearchResults);

        // assert
        mappedResult.Should().NotBeNull();
        mappedResult.Values.Should().HaveCount(searchResultDocuments.Count);

        foreach (SearchResult<NationalPupilDatabaseLearnerDataTransferObject> searchResult in searchResultDocuments)
        {
            NationalPupilDatabaseLearner? learner =
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
        List<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>> emptySearchResultDocuments =
            new SearchResultFakeBuilder<NationalPupilDatabaseLearnerDataTransferObject>()
                .WithEmptySearchResult()
                .Create();

        // act
        NationalPupilDatabaseLearners? result =
            _sut.Map(
                PageableTestDouble.FromResults(emptySearchResultDocuments));

        // assert
        result.Should().NotBeNull();
        result.Values.Should().HaveCount(0);
    }

    [Fact]
    public void Map_WithNullSearchResults_ThrowsArgumentNullException()
    {
        // act.
        _sut
            .Invoking(mapper =>
                mapper.Map(null!))
                    .Should()
                        .Throw<ArgumentNullException>()
                        .WithMessage("Value cannot be null. (Parameter 'input')");
    }

    [Fact]
    public void Map_WithANullSearchResult_SkipsNullDocuments()
    {
        // arrange
        List<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>> searchResultDocuments =
            new SearchResultFakeBuilder<NationalPupilDatabaseLearnerDataTransferObject>()
                .IncludeNullDocument()
                .Create();

        Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>> pageableSearchResults =
            PageableTestDouble.FromResults(searchResultDocuments);

        // act
        NationalPupilDatabaseLearners result = _sut.Map(pageableSearchResults);

        // assert
        result.Should().NotBeNull();

        // null document entries must be excluded
        result.Values.Should().HaveCount(
            searchResultDocuments.Count(sr => sr.Document != null));
    }
}
