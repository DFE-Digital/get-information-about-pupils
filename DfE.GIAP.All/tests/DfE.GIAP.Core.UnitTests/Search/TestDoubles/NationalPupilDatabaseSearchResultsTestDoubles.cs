using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.SharedTests.TestDoubles.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;
internal static class NationalPupilDatabaseSearchResultsTestDoubles
{
    public static ISearchResults<NationalPupilDatabaseLearners, SearchFacets> Stub() =>
        new()
        {
            Results = new(NationalPupilDatabaseLearnerTestDoubles.FakeMany()),           // Populated learner results
            FacetResults = SearchFacetsTestDouble.Stub()   // Populated facet results
        };

    public static ISearchResults<NationalPupilDatabaseLearners, SearchFacets> StubWithNoResults() =>
        new()
        {
            Results = new([]),      // Unpopulated learner results
            FacetResults = SearchFacetsTestDouble.Stub()   // Populated facet results
        };
}
