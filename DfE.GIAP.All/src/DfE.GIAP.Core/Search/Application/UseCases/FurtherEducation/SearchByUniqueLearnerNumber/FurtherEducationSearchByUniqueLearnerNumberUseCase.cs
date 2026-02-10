using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByUniqueLearnerNumber;
internal sealed class FurtherEducationSearchByUniqueLearnerNumberUseCase : IUseCase<FurtherEducationSearchByUniqueLearnerNumberRequest, FurtherEducationSearchByUniqueLearnerNumberResponse>
{
    private readonly ISearchLearnersByIdentifierService<FurtherEducationLearners> _searchLearnerByIdentifierService;

    public FurtherEducationSearchByUniqueLearnerNumberUseCase(ISearchLearnersByIdentifierService<FurtherEducationLearners> searchLearnerByIdentifierService)
    {
        ArgumentNullException.ThrowIfNull(searchLearnerByIdentifierService);
        _searchLearnerByIdentifierService = searchLearnerByIdentifierService;
    }
    public async Task<FurtherEducationSearchByUniqueLearnerNumberResponse> HandleRequestAsync(FurtherEducationSearchByUniqueLearnerNumberRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        SearchResponse<FurtherEducationLearners, SearchFacets> searchResponse = await _searchLearnerByIdentifierService.SearchAsync(
            new SearchLearnersByIdentifierRequest(
                request.UniqueLearnerNumbers!,
                request.SearchCriteria!,
                request.Sort!,
                request.Offset));

        return new FurtherEducationSearchByUniqueLearnerNumberResponse(
            searchResponse.LearnerSearchResults,
            searchResponse.TotalNumberOfResults);
    }
}
