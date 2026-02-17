using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
internal sealed class PupilPremiumSearchByUniquePupilNumberUseCase : IUseCase<PupilPremiumSearchByUniquePupilNumberRequest, SearchResponse<PupilPremiumLearners>>
{
    private readonly ISearchLearnersByIdentifierService<PupilPremiumLearners> _searchLearnerByIdentifierService;

    public PupilPremiumSearchByUniquePupilNumberUseCase(ISearchLearnersByIdentifierService<PupilPremiumLearners> searchLearnerByIdentifierService)
    {
        ArgumentNullException.ThrowIfNull(searchLearnerByIdentifierService);
        _searchLearnerByIdentifierService = searchLearnerByIdentifierService;
    }

    public async Task<SearchResponse<PupilPremiumLearners>> HandleRequestAsync(PupilPremiumSearchByUniquePupilNumberRequest request)
    {
        SearchServiceResponse<PupilPremiumLearners, SearchFacets> response =
            await _searchLearnerByIdentifierService.SearchAsync(
                new SearchLearnersByIdentifierRequest(
                    identifiers: request.UniquePupilNumbers!,
                    searchCriteria: request.SearchCriteria!,
                    sort: request.Sort!,
                    offset: request.Offset));

        return SearchResponse<PupilPremiumLearners>.Create(
            response.LearnerSearchResults ?? PupilPremiumLearners.CreateEmpty(),
            totalResults: response.TotalNumberOfResults);
    }
}
