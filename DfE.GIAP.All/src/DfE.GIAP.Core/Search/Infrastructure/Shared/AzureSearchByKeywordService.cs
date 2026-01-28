using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Builders;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options.Extensions;
using Microsoft.Extensions.Options;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.Search.Infrastructure.Shared;
internal sealed class AzureSearchByKeywordService : IAzureSearchByKeywordService
{
    private readonly ISearchByKeywordService _searchByKeywordService;
    private readonly IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets> _facetsMapper;
    private readonly AzureSearchOptions _azureSearchOptions;
    private readonly ISearchOptionsBuilder _searchOptionsBuilder;

    public AzureSearchByKeywordService(
        ISearchByKeywordService searchByKeywordService,
        IOptions<AzureSearchOptions> azureSearchOptions,
        IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets> facetsMapper,
        ISearchOptionsBuilder searchOptionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(azureSearchOptions);
        ArgumentNullException.ThrowIfNull(azureSearchOptions?.Value);
        _azureSearchOptions = azureSearchOptions.Value;

        ArgumentNullException.ThrowIfNull(searchByKeywordService);
        _searchByKeywordService = searchByKeywordService;

        ArgumentNullException.ThrowIfNull(facetsMapper);
        _facetsMapper = facetsMapper;

        ArgumentNullException.ThrowIfNull(searchOptionsBuilder);
        _searchOptionsBuilder = searchOptionsBuilder;
    }

    public async Task<SearchResults<TOutputModel, SearchFacets>> SearchByKeywordAsync<TDto, TOutputModel>(
        SearchServiceAdapterRequest request,
        IMapper<Pageable<SearchResult<TDto>>, TOutputModel> dtoToOutputModelMapper) where TDto : class
    {
        SearchIndexOptions indexOptions = _azureSearchOptions.GetIndexOptions(request.SearchIndexKey);

        SearchOptions searchOptions =
            _searchOptionsBuilder
                .WithSearchMode((SearchMode)indexOptions.SearchMode)
                .WithSize(indexOptions.Size)
                .WithOffset(request.Offset)
                .WithIncludeTotalCount(indexOptions.IncludeTotalCount)
                .WithSearchFields(request.SearchFields)
                .WithFacets(request.Facets)
                .WithFilters(request.SearchFilterRequests)
                .WithSortOrder(request.SortOrdering)
                .Build();

        Response<SearchResults<TDto>> searchResults =
            await _searchByKeywordService.SearchAsync<TDto>(
                request.SearchKeyword,
                indexOptions.SearchIndex,
                searchOptions
            ) ?? throw new InvalidOperationException(
                $"Unable to derive search results based on input {request.SearchKeyword}.");

        return new SearchResults<TOutputModel, SearchFacets>
        {
            Results = dtoToOutputModelMapper.Map(searchResults.Value.GetResults()),
            FacetResults = searchResults.Value.Facets != null
                ? _facetsMapper.Map(searchResults.Value.Facets.ToDictionary()) : null
        };
    }
}
