using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Shared;

namespace DfE.GIAP.Core.Search.Infrastructure.PupilPremium;
internal sealed class PupilPremiumAzureSearchServiceAdaptor : ISearchServiceAdapter<PupilPremiumLearners, SearchFacets>
{
    private readonly IMapper<Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners> _searchResultMapper;
    private readonly IAzureSearchByKeywordService _azureSearchByKeywordService;

    public PupilPremiumAzureSearchServiceAdaptor(
        IMapper<Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners> searchResultMapper,
        IAzureSearchByKeywordService azureSearchByKeywordService)
    {
        ArgumentNullException.ThrowIfNull(searchResultMapper);
        _searchResultMapper = searchResultMapper;

        ArgumentNullException.ThrowIfNull(azureSearchByKeywordService);
        _azureSearchByKeywordService = azureSearchByKeywordService;
    }

    public Task<SearchResults<PupilPremiumLearners, SearchFacets>> SearchAsync(SearchServiceAdapterRequest searchServiceAdapterRequest)
    {
        return
            _azureSearchByKeywordService.SearchByKeywordAsync(
                searchServiceAdapterRequest,
                _searchResultMapper);
    }
}
