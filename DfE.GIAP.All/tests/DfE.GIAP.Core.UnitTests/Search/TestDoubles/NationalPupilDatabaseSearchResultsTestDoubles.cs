using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;
internal static class NationalPupilDatabaseSearchResultsTestDoubles
{
    public static SearchServiceAdaptorResponse<NationalPupilDatabaseLearners, SearchFacets> Stub() =>
        new()
        {
            Results = new(NationalPupilDatabaseLearnerTestDoubles.FakeMany()),           // Populated learner results
            FacetResults = SearchFacetsTestDouble.Stub()   // Populated facet results
        };

    public static SearchServiceAdaptorResponse<NationalPupilDatabaseLearners, SearchFacets> StubWithNoResults() =>
        new()
        {
            Results = new([]),      // Unpopulated learner results
            FacetResults = SearchFacets.CreateEmpty()   // Empty facet results
        };
}
