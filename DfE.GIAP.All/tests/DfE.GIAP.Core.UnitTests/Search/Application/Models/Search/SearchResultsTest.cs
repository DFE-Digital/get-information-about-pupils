﻿using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;
using Model = DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Search;

public sealed class SearchResultsTests
{
    [Fact]
    public void Properties_CanBeInitializedViaObjectInitializer()
    {
        // arrange
        Learners learners = new(new List<Model.Learner>
        {
            new(
                new LearnerIdentifier("1234567890"),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    new DateTime(2005, 6, 1),
                    LearnerCharacteristics.Gender.Female))
        });

        SearchFacets facets =
            new(new List<SearchFacet>
            {
                new("Region", [new FacetResult("North", 10)])
            });

        // act
        SearchResults<Learners, SearchFacets> result =
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
        SearchResults<Learners, SearchFacets> result = new();

        // Assert
        result.Results.Should().BeNull();
        result.FacetResults.Should().BeNull();
    }

    [Fact]
    public void CanInstantiateWithDifferentGenericTypes()
    {
        // arrange
        List<string> dummyResults = ["A", "B"];
        Dictionary<string, int> dummyFacets = new() { { "X", 1 }, { "Y", 2 } };

        // act
        SearchResults<List<string>, Dictionary<string, int>> result = new()
        {
            Results = dummyResults,
            FacetResults = dummyFacets
        };

        // Assert
        result.Results.Should().BeEquivalentTo(dummyResults);
        result.FacetResults.Should().BeEquivalentTo(dummyFacets);
    }
}

