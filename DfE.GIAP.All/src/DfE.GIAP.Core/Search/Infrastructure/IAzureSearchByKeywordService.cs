using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Infrastructure;
public interface IAzureSearchByKeywordService
{
    Task<SearchResults<TOutputModel, SearchFacets>>
        SearchByKeywordAsync<TDto, TOutputModel>(
            SearchServiceAdapterRequest request,
            IMapper<Pageable<SearchResult<TDto>>, TOutputModel> dtoToOutputModelMapper) where TDto : class;
}
