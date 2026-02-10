using DfE.GIAP.Core.Search.Application.Models.Search.Facets;

namespace DfE.GIAP.Core.Search.Application.Services.SearchByName;
public interface ISearchLearnerByNameService<TSearchResponseModel>
{
    Task<SearchResponse<TSearchResponseModel, SearchFacets>> SearchAsync(SearchLearnerByNameRequest request);
}
