using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
internal sealed class NationalPupilDatabaseSearchUseCase : IUseCase<NationalPupilDatabaseSearchRequest, NationalPupilDatabaseSearchResponse>
{
    private readonly SearchCriteria _searchCriteria;
    private readonly ISearchServiceAdapter<NationalPupilDatabaseLearners, SearchFacets> _searchServiceAdapter;

    public NationalPupilDatabaseSearchUseCase(
        ISearchCriteriaProvider searchCriteriaProvider,
        ISearchServiceAdapter<NationalPupilDatabaseLearners, SearchFacets> searchServiceAdapter)
    {
        ArgumentNullException.ThrowIfNull(searchCriteriaProvider);
        _searchCriteria = searchCriteriaProvider.GetCriteria("npd");

        ArgumentNullException.ThrowIfNull(searchServiceAdapter);
        _searchServiceAdapter = searchServiceAdapter;
    }
    public async Task<NationalPupilDatabaseSearchResponse> HandleRequestAsync(NationalPupilDatabaseSearchRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SearchKeywords))
        {
            return new(SearchResponseStatus.InvalidRequest);
        }

        try
        {
            SearchResults<NationalPupilDatabaseLearners, SearchFacets>? searchResults =
                await _searchServiceAdapter.SearchAsync(
                    new SearchServiceAdapterRequest(
                        searchIndexKey: "npd",
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
                    FacetedResults = searchResults.FacetResults
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
