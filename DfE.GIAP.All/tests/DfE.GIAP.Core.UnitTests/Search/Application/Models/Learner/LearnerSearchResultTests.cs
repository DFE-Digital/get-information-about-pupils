using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;
using Model = DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnerSearchResultTests
{
    [Fact]
    public void Constructor_WithStatus_ShouldSetStatusProperty()
    {
        // Arrange
        SearchResponseStatus status = SearchResponseStatus.Success;

        // Act
        LearnerSearchResult result = new(status);

        // Assert
        result.Status.Should().Be(status);
    }

    [Fact]
    public void Properties_CanBeInitializedViaObjectInitializer()
    {
        // Arrange
        Learners learners = new([
            new Model.Learner(
                new LearnerIdentifier("1234567890"),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    new DateTime(2005, 6, 1),
                    LearnerCharacteristics.Gender.Female))
        ]);

        SearchFacets facets =
            new(new List<SearchFacet>
            {
                new("Region", [new FacetResult("North", 10)])
            });

        LearnerSearchResult result =
            new(SearchResponseStatus.Success)
            {
                LearnerResults = learners,
                FacetedResults = facets,
                TotalNumberOfLearners = learners.Count
            };

        // Assert
        result.LearnerResults.Should().Be(learners);
        result.FacetedResults.Should().Be(facets);
        result.TotalNumberOfLearners.Should().Be(learners.Count);
    }

    [Fact]
    public void Properties_WhenUninitialized_ShouldBeNullOrZero()
    {
        // Act
        LearnerSearchResult result = new(SearchResponseStatus.NoResultsFound);

        // Assert
        result.LearnerResults.Should().BeNull();
        result.FacetedResults.Should().BeNull();
        result.TotalNumberOfLearners.Should().Be(0);
    }
}

