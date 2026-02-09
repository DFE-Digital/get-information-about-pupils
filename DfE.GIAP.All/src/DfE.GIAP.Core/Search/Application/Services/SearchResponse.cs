using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Application.Services;
internal record SearchResponse<TResponse, TFacets>
{
    public SearchResponse(SearchResponseStatus status, int? totalNumberOfResults = null)
    {
        Status = status;
        TotalNumberOfResults = new(totalNumberOfResults);
    }

    public TResponse? LearnerSearchResults { get; init; }
    public TFacets? FacetedResults { get; init; }
    public SearchResponseStatus Status { get; }
    public SearchResultCount TotalNumberOfResults { get; }
}
