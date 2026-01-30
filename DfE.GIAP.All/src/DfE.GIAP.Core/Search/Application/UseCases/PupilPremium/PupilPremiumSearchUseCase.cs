using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
internal sealed class PupilPremiumSearchUseCase : IUseCase<PupilPremiumSearchRequest, PupilPremiumSearchResponse>
{
    private readonly SearchCriteria _searchCriteria;
    private readonly ISearchServiceAdapter<PupilPremiumLearners, SearchFacets> _searchServiceAdapter;

    public PupilPremiumSearchUseCase(
        ISearchCriteriaProvider searchCriteriaProvider,
        ISearchServiceAdapter<PupilPremiumLearners, SearchFacets> searchServiceAdapter)
    {
        ArgumentNullException.ThrowIfNull(searchCriteriaProvider);
        _searchCriteria = searchCriteriaProvider.GetCriteria(key: "pupil-premium");

        ArgumentNullException.ThrowIfNull(searchServiceAdapter);
        _searchServiceAdapter = searchServiceAdapter;
    }
    public async Task<PupilPremiumSearchResponse> HandleRequestAsync(PupilPremiumSearchRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SearchKeywords))
        {
            return new(SearchResponseStatus.InvalidRequest);
        }

        try
        {
            SearchResults<PupilPremiumLearners, SearchFacets>? searchResults =
                await _searchServiceAdapter.SearchAsync(
                    new SearchServiceAdapterRequest(
                        searchIndexKey: "pupil-premium",
                        request.SearchKeywords,
                        _searchCriteria.SearchFields,
                        request.SortOrder,
                        _searchCriteria.Facets,
                        request.FilterRequests,
                        request.Offset));

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
