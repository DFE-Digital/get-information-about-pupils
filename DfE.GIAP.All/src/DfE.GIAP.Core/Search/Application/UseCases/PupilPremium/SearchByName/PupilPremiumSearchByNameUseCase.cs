using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByName;
internal sealed class PupilPremiumSearchByNameUseCase : IUseCase<PupilPremiumSearchByNameRequest, SearchResponse<PupilPremiumLearners>>
{
    private readonly ISearchLearnerByNameService<PupilPremiumLearners> _searchForLearnerByNameService;

    public PupilPremiumSearchByNameUseCase(
        ISearchLearnerByNameService<PupilPremiumLearners> searchForLearnerByNameService)
    {
        ArgumentNullException.ThrowIfNull(searchForLearnerByNameService);
        _searchForLearnerByNameService = searchForLearnerByNameService;
    }

    public async Task<SearchResponse<PupilPremiumLearners>> HandleRequestAsync(PupilPremiumSearchByNameRequest request)
    {
        SearchServiceResponse<PupilPremiumLearners, SearchFacets> response =
            await _searchForLearnerByNameService.SearchAsync(
                new SearchLearnerByNameRequest(
                    searchKeywords: request.SearchKeywords!,
                    searchCriteria: request.SearchCriteria!,
                    sortOrder: request.SortOrder!,
                    filters: request.FilterRequests,
                    offset: request.Offset));

        return SearchResponse<PupilPremiumLearners>.Create(
            response.LearnerSearchResults ?? PupilPremiumLearners.CreateEmpty(),
            response.FacetedResults,
            response.TotalNumberOfResults);
    }
}
