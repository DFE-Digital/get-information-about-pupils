using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
internal sealed class NationalPupilDatabaseSearchByUniquePupilNumberUseCase : IUseCase<NationalPupilDatabaseSearchByUniquePupilNumberRequest, NationalPupilDatabaseSearchByUniquePupilNumberResponse>
{
    private readonly ISearchLearnersByIdentifierService<NationalPupilDatabaseLearners> _searchLearnersByIdentifierService;

    public NationalPupilDatabaseSearchByUniquePupilNumberUseCase(ISearchLearnersByIdentifierService<NationalPupilDatabaseLearners> searchLearnersByIdentifierService)
    {
        ArgumentNullException.ThrowIfNull(searchLearnersByIdentifierService);
        _searchLearnersByIdentifierService = searchLearnersByIdentifierService;
    }
    public async Task<NationalPupilDatabaseSearchByUniquePupilNumberResponse> HandleRequestAsync(NationalPupilDatabaseSearchByUniquePupilNumberRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        SearchResponse<NationalPupilDatabaseLearners, SearchFacets> response =
            await _searchLearnersByIdentifierService.SearchAsync(
                new SearchLearnersByIdentifierRequest(
                    identifiers: request.UniquePupilNumbers!,
                    searchCriteria: request.SearchCriteria!,
                    sort: request.Sort!,
                    offset: request.Offset));

            return new NationalPupilDatabaseSearchByUniquePupilNumberResponse(
                    response.LearnerSearchResults,
                    response.TotalNumberOfResults);
    }
}
