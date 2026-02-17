using DfE.GIAP.Core.Search.Application.Models.Search.Facets;

namespace DfE.GIAP.Core.Search.Application.Models.Search;
public sealed class SearchResponse<TResponse> where TResponse : class
{
    public SearchResponse(TResponse response, SearchFacets? facets = null, int? totalResults = null)
    {
        ArgumentNullException.ThrowIfNull(response);
        LearnerSearchResults = response;
        FacetedResults = facets ?? SearchFacets.CreateEmpty();
        TotalNumberOfResults = new(totalResults);
    }

    public TResponse LearnerSearchResults { get; }

    public SearchFacets FacetedResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }

    public static SearchResponse<TResponse> Create(
        TResponse response,
        SearchFacets? facets = null,
        int? totalResults = null) => new(response, facets, totalResults);
}
