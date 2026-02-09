using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
internal sealed class PupilPremiumSearchByUniquePupilNumberUseCase : IUseCase<PupilPremiumSearchByUniquePupilNumberRequest, PupilPremiumSearchByUniquePupilNumberResponse>
{
    private readonly ISearchLearnersByIdentifierService<PupilPremiumLearners> _searchLearnerByIdentifierService;

    public PupilPremiumSearchByUniquePupilNumberUseCase(ISearchLearnersByIdentifierService<PupilPremiumLearners> searchLearnerByIdentifierService)
    {
        ArgumentNullException.ThrowIfNull(searchLearnerByIdentifierService);
        _searchLearnerByIdentifierService = searchLearnerByIdentifierService;
    }

    public async Task<PupilPremiumSearchByUniquePupilNumberResponse> HandleRequestAsync(PupilPremiumSearchByUniquePupilNumberRequest request)
    {
        SearchResponse<PupilPremiumLearners, SearchFacets> response =
            await _searchLearnerByIdentifierService.SearchAsync(
                new SearchLearnersByIdentifierRequest(
                    identifiers: request.UniquePupilNumbers!,
                    searchCriteria: request.SearchCriteria!,
                    sort: request.Sort!,
                    offset: request.Offset));

        return new PupilPremiumSearchByUniquePupilNumberResponse(response.LearnerSearchResults, response.TotalNumberOfResults);
    }
}
