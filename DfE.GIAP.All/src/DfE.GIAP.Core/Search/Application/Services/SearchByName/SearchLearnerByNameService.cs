using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;

namespace DfE.GIAP.Core.Search.Application.Services.SearchByName;
internal sealed class SearchLearnerByNameService<TSearchResponse> : ISearchLearnerByNameService<TSearchResponse>
    where TSearchResponse : IHasSearchResults
{
    private readonly ISearchServiceAdapter<TSearchResponse, SearchFacets> _searchServiceAdapter;

    public SearchLearnerByNameService(
        ISearchServiceAdapter<TSearchResponse, SearchFacets> searchServiceAdapter)
    {
        ArgumentNullException.ThrowIfNull(searchServiceAdapter);
        _searchServiceAdapter = searchServiceAdapter;
    }

    public async Task<SearchResponse<TSearchResponse, SearchFacets>> SearchAsync(SearchLearnerByNameRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SearchKeywords))
        {
            return new(SearchResponseStatus.InvalidRequest);
        }

        try
        {
            ISearchResults<TSearchResponse, SearchFacets>? searchResults =
                await _searchServiceAdapter.SearchAsync(
                    new SearchServiceAdapterRequest(
                        index: request.SearchCriteria.Index,
                        searchKeyword: request.SearchKeywords,
                        searchFields: request.SearchCriteria.SearchFields,
                        sortOrdering: request.SortOrder,
                        resultsSize: request.SearchCriteria.Size,
                        facets: request.SearchCriteria.Facets,
                        searchFilterRequests: request.FilterRequests,
                        includeTotalCount: request.SearchCriteria.IncludeTotalCount,
                        offset: request.Offset));


            return searchResults?.Results?.Count > 0
                ? new(SearchResponseStatus.Success, searchResults.Results.Count)
                {
                    LearnerSearchResults = searchResults.Results,
                    FacetedResults = searchResults.FacetResults,
                }
                : new(SearchResponseStatus.NoResultsFound);

        }
        catch (Exception)
        {
            // Handles unexpected failures such as adapter exceptions or infrastructure issues.
            return new(SearchResponseStatus.SearchServiceError);
        }
    }
}