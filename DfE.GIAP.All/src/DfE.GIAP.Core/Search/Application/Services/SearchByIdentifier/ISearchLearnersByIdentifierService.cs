using DfE.GIAP.Core.Search.Application.Models.Search.Facets;

namespace DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;

internal interface ISearchLearnersByIdentifierService<TSearchResponseModel> where TSearchResponseModel : IHasSearchResults
{
    Task<SearchResponse<TSearchResponseModel, SearchFacets>> SearchAsync(SearchLearnersByIdentifierRequest request);
}