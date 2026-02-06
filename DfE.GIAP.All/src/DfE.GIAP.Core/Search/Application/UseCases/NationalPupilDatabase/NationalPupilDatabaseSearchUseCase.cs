using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
internal sealed class NationalPupilDatabaseSearchUseCase : IUseCase<NationalPupilDatabaseSearchRequest, NationalPupilDatabaseSearchResponse>
{
    private readonly ISearchServiceAdapter<NationalPupilDatabaseLearners, SearchFacets> _searchServiceAdapter;

    public NationalPupilDatabaseSearchUseCase(
        ISearchServiceAdapter<NationalPupilDatabaseLearners, SearchFacets> searchServiceAdapter)
    {
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
                        request.SearchIndexKey,
                        request.SearchKeywords,
                        request.SearchCriteria.SearchFields,
                        request.SortOrder,
                        request.SearchCriteria.Facets,
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
