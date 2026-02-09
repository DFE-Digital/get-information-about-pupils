using DfE.GIAP.Core.Search.Application.Models.Search.Facets;

namespace DfE.GIAP.Core.Search.Application.Services.SearchByName;
internal interface ISearchLearnerByNameService<TSearchResponseModel>
{
    Task<SearchResponse<TSearchResponseModel, SearchFacets>> SearchAsync(SearchLearnerByNameRequest request);
}