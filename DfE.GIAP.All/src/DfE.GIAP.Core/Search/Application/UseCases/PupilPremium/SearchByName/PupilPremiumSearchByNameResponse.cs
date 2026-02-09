using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByName;
public record PupilPremiumSearchByNameResponse
{
    internal PupilPremiumSearchByNameResponse(PupilPremiumLearners? learners, SearchFacets? facets, int totalResultsCount)
    {
        LearnerSearchResults = learners ?? PupilPremiumLearners.CreateEmpty();
        FacetedResults = facets ?? SearchFacets.CreateEmpty();
        TotalNumberOfResults = new(totalResultsCount);
    }

    public PupilPremiumLearners LearnerSearchResults { get; }

    public SearchFacets FacetedResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}
