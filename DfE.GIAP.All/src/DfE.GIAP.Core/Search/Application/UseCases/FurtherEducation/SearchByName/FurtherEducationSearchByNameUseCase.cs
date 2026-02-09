using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.Services;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
internal sealed class FurtherEducationSearchByNameUseCase : IUseCase<FurtherEducationSearchByNameRequest, FurtherEducationSearchByNameResponse>
{
    private readonly ISearchLearnerByNameService<FurtherEducationLearners> _searchForLearnerByNameService;

    public FurtherEducationSearchByNameUseCase(ISearchLearnerByNameService<FurtherEducationLearners> searchForLearnerByNameService)
    {
        ArgumentNullException.ThrowIfNull(searchForLearnerByNameService);
        _searchForLearnerByNameService = searchForLearnerByNameService;
    }

    public async Task<FurtherEducationSearchByNameResponse> HandleRequestAsync(FurtherEducationSearchByNameRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        SearchResponse<FurtherEducationLearners, SearchFacets> response = await _searchForLearnerByNameService.SearchAsync(
            new SearchLearnerByNameRequest(
                request.SearchKeywords!,
                request.SearchCriteria!, 
                request.SortOrder!,
                request.FilterRequests,
                request.Offset));

        return new FurtherEducationSearchByNameResponse(
            response.LearnerSearchResults, 
            response.FacetedResults, 
            response.TotalNumberOfResults);
    }
}
