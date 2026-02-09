using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;

/// <summary>
/// Provides test doubles for <see cref="ISearchResults{TResults, TFacets}"/> used in unit tests.
/// Enables deterministic testing of search adapter logic, result mapping, and facet handling.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FurtherEducationSearchResultsTestDoubles
{
    /// <summary>
    /// Creates a stubbed <see cref="ISearchResults{Learners, SearchFacets}"/> instance
    /// populated with synthetic learner results and facet data.
    /// Useful for simulating successful search responses in test scenarios.
    /// </summary>
    public static SearchResults<FurtherEducationLearners, SearchFacets> Stub() =>
        new()
        {
            Results = FurtherEducationLearnersTestDouble.Stub(),           // Populated learner results
            FacetResults = SearchFacetsTestDouble.Stub()   // Populated facet results
        };

    /// <summary>
    /// Creates an empty <see cref="ISearchResults{Learners, SearchFacets}"/> instance.
    /// Useful for testing fallback logic, empty state handling, and edge cases.
    /// </summary>
    public static SearchResults<FurtherEducationLearners, SearchFacets> StubWithNoResults() =>
        new()
        {
            Results = FurtherEducationLearnersTestDouble.EmptyStub(),      // Unpopulated learner results
            FacetResults = SearchFacets.CreateEmpty()   // Empty facet results
        };
}
