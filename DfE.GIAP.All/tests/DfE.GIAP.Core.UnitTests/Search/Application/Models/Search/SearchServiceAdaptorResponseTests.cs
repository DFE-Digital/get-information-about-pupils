using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Search;

public sealed class SearchServiceAdaptorResponseTests
{
    [Fact]
    public void Properties_CanBeInitializedViaObjectInitializer()
    {
        // arrange
        FurtherEducationLearners learners = FurtherEducationLearnersTestDouble.Stub();

        SearchFacets facets = SearchFacetsTestDouble.Stub();

        // act
        SearchServiceAdaptorResponse<FurtherEducationLearners, SearchFacets> result =
            new()
            {
                Results = learners,
                FacetResults = facets
            };

        // Assert
        result.Results.Should().Be(learners);
        result.FacetResults.Should().Be(facets);
    }

    [Fact]
    public void Properties_WhenUninitialized_ShouldBeNull()
    {
        // act
        SearchServiceAdaptorResponse<FurtherEducationLearners, SearchFacets> result = new();

        // Assert
        result.Results.Should().BeNull();
        result.FacetResults.Should().BeNull();
    }

    [Fact]
    public void CanInstantiateWithDifferentGenericTypes()
    {
        // arrange
        List<string> dummyResults = ["A", "B"];
        Dictionary<string, int> dummyFacets = new()
        {
            { "X", 1 },
            { "Y", 2 }
        };

        // act
        SearchServiceAdaptorResponse<List<string>, Dictionary<string, int>> result = new()
        {
            Results = dummyResults,
            FacetResults = dummyFacets
        };

        // Assert
        result.Results.Should().BeEquivalentTo(dummyResults);
        result.FacetResults.Should().BeEquivalentTo(dummyFacets);
    }
}

