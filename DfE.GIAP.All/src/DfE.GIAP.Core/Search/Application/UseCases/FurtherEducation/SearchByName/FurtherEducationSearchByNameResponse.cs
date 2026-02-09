using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;

public record FurtherEducationSearchByNameResponse
{
    internal FurtherEducationSearchByNameResponse(FurtherEducationLearners? learners, SearchFacets? facets, int totalNumberOfResults)
    {
        LearnerSearchResults = learners ?? FurtherEducationLearners.CreateEmpty();
        FacetedResults = facets ?? SearchFacets.CreateEmpty();
        TotalNumberOfResults = new(totalNumberOfResults);
    }

    public FurtherEducationLearners LearnerSearchResults { get; }

    public SearchFacets FacetedResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}
