using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;


// Import model representing search result payload including facets
using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;

/// <summary>
/// Provides test doubles for <see cref="SearchResults{TResults, TFacets}"/> used in unit tests.
/// Enables deterministic testing of search adapter logic, result mapping, and facet handling.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SearchResultsTestDouble
{
    /// <summary>
    /// Creates a stubbed <see cref="SearchResults{Learners, SearchFacets}"/> instance
    /// populated with synthetic learner results and facet data.
    /// Useful for simulating successful search responses in test scenarios.
    /// </summary>
    public static SearchResults<FurtherEducationLearners, SearchFacets> Stub() =>
        new()
        {
            Results = LearnersTestDouble.Stub(),           // Populated learner results
            FacetResults = SearchFacetsTestDouble.Stub()   // Populated facet results
        };

    /// <summary>
    /// Creates an empty <see cref="SearchResults{Learners, SearchFacets}"/> instance.
    /// Useful for testing fallback logic, empty state handling, and edge cases.
    /// </summary>
    public static SearchResults<FurtherEducationLearners, SearchFacets> StubWithNoResults() =>
        new()
        {
            Results = LearnersTestDouble.EmptyStub(),      // Unpopulated learner results
            FacetResults = SearchFacetsTestDouble.Stub()   // Populated facet results
        };
}
