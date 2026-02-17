using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
internal sealed class NationalPupilDatabaseSearchByNameUseCase : IUseCase<NationalPupilDatabaseSearchByNameRequest, SearchResponse<NationalPupilDatabaseLearners>>
{
    private readonly ISearchLearnerByNameService<NationalPupilDatabaseLearners> _searchForLearnerByNameService;

    public NationalPupilDatabaseSearchByNameUseCase(
        ISearchLearnerByNameService<NationalPupilDatabaseLearners> searchForLearnerByNameService)
    {
        ArgumentNullException.ThrowIfNull(searchForLearnerByNameService);
        _searchForLearnerByNameService = searchForLearnerByNameService;
    }

    public async Task<SearchResponse<NationalPupilDatabaseLearners>> HandleRequestAsync(NationalPupilDatabaseSearchByNameRequest request)
    {
        SearchServiceResponse<NationalPupilDatabaseLearners, SearchFacets> response =
            await _searchForLearnerByNameService.SearchAsync(
                new SearchLearnerByNameRequest(
                    request.SearchKeywords!,
                    request.SearchCriteria!,
                    request.SortOrder!,
                    request.FilterRequests,
                    request.Offset));

        return SearchResponse<NationalPupilDatabaseLearners>.Create(
            response.LearnerSearchResults ?? NationalPupilDatabaseLearners.CreateEmpty(),
            response.FacetedResults,
            response.TotalNumberOfResults);
    }
}
