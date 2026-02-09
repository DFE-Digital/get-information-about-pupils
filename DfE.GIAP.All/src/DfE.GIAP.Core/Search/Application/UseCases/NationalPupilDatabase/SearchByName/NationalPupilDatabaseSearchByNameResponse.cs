using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
public record NationalPupilDatabaseSearchByNameResponse
{
    public NationalPupilDatabaseSearchByNameResponse(NationalPupilDatabaseLearners? learners, SearchFacets? facets, SearchResultCount searchResultCount)
    {
        LearnerSearchResults = learners ?? NationalPupilDatabaseLearners.CreateEmpty();
        FacetedResults = facets ?? SearchFacets.CreateEmpty();
        TotalNumberOfResults = searchResultCount;
    }

    public NationalPupilDatabaseLearners LearnerSearchResults { get; }

    public SearchFacets FacetedResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}
