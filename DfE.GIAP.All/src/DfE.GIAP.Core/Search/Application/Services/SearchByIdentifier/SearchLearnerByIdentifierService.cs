using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;

namespace DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
internal sealed class SearchLearnerByIdentifierService<TResponse> : ISearchLearnersByIdentifierService<TResponse>
    where TResponse : IHasSearchResults
{
    private readonly ISearchServiceAdapter<TResponse, SearchFacets> _searchServiceAdapter;

    public SearchLearnerByIdentifierService(ISearchServiceAdapter<TResponse, SearchFacets> searchServiceAdapter)
    {
        ArgumentNullException.ThrowIfNull(searchServiceAdapter);
        _searchServiceAdapter = searchServiceAdapter;
    }

    public async Task<SearchServiceResponse<TResponse, SearchFacets>> SearchAsync(SearchLearnersByIdentifierRequest request)
    {
        if (request == null || request.Identifiers.Count == 0)
        {
            return new SearchServiceResponse<TResponse, SearchFacets>(SearchResponseStatus.InvalidRequest);
        }

        // TODO understand if this is neccesary when we are searching for the identifier in `SearchFields`?
        FilterRequest? filterByIdentifier =
            request.SearchCriteria.SearchFields.Count == 1 ?
             new FilterRequest(
                 filterName: request.SearchCriteria.SearchFields[0],
                 filterValues: request.Identifiers.Select(t => (object)t).ToList())
                : null;

        try
        {
            ISearchServiceAdaptorResponse<TResponse, SearchFacets>? searchResults =
                await _searchServiceAdapter.SearchAsync(
                    new SearchServiceAdapterRequest(
                        index: request.SearchCriteria.Index,
                        searchKeyword: string.Join(" AND ", request.Identifiers),
                        searchFields: request.SearchCriteria.SearchFields,
                        sortOrdering: request.Sort,
                        size: request.SearchCriteria.Size,
                        facets: request.SearchCriteria.Facets,
                        searchFilterRequests: filterByIdentifier is null ? null : [filterByIdentifier],
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
