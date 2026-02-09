using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;
public static class PupilPremiumSearchResultsTestDoubles
{
    public static SearchResults<PupilPremiumLearners, SearchFacets> Stub()
    {
        return new()
        {
            Results = new PupilPremiumLearners(PupilPremiumLearnerTestDoubles.FakeMany()),
            FacetResults = SearchFacetsTestDouble.Stub()
        };
    }

    public static SearchResults<PupilPremiumLearners, SearchFacets> StubWithNoResults()
    {
        return new()
        {
            Results = new PupilPremiumLearners([]),
            FacetResults = SearchFacets.CreateEmpty()
        };
    }
}
