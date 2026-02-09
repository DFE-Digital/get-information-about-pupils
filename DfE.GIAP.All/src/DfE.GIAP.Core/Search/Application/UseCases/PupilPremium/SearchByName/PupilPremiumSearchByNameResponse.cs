using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByName;
public record PupilPremiumSearchByNameResponse
{
    internal PupilPremiumSearchByNameResponse(PupilPremiumLearners? learners, SearchFacets? facets, SearchResultCount count)
    {
        LearnerSearchResults = learners ?? PupilPremiumLearners.CreateEmpty();
        FacetedResults = facets ?? SearchFacets.CreateEmpty();
        TotalNumberOfResults = count;
    }

    public PupilPremiumLearners LearnerSearchResults { get; }

    public SearchFacets FacetedResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}