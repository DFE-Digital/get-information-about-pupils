using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Shared;

namespace DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase;
internal sealed class NationalPupilDatabaseAzureSearchServiceAdaptor : ISearchServiceAdapter<NationalPupilDatabaseLearners, SearchFacets>
{
    private readonly IAzureSearchByKeywordService _searchByKeywordService;
    private readonly IMapper<Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>>, NationalPupilDatabaseLearners> _dtoToModelMapper;

    public NationalPupilDatabaseAzureSearchServiceAdaptor(
        IAzureSearchByKeywordService searchByKeywordService,
        IMapper<Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>>, NationalPupilDatabaseLearners> mapper)
    {
        ArgumentNullException.ThrowIfNull(searchByKeywordService);
        _searchByKeywordService = searchByKeywordService;

        ArgumentNullException.ThrowIfNull(mapper);
        _dtoToModelMapper = mapper;
    }

    public Task<SearchResults<NationalPupilDatabaseLearners, SearchFacets>> SearchAsync(SearchServiceAdapterRequest searchServiceAdapterRequest)
    {
        return
            _searchByKeywordService.SearchByKeywordAsync(
                searchServiceAdapterRequest,
                _dtoToModelMapper);
    }
}
