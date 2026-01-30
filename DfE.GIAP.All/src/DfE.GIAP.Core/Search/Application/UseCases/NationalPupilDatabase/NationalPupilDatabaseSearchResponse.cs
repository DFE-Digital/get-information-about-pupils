using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
public record NationalPupilDatabaseSearchResponse
{
    public NationalPupilDatabaseSearchResponse(SearchResponseStatus status, int? totalNumberOfResults = null)
    {
        Status = status;
        TotalNumberOfResults = new(totalNumberOfResults);
    }

    public NationalPupilDatabaseLearners? LearnerSearchResults { get; init; }

    public SearchFacets? FacetedResults { get; init; }

    public SearchResponseStatus Status { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}
