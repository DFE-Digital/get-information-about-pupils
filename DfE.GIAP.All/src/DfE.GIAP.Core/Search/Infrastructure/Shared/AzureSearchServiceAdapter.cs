using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Builders;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options.Extensions;
using Microsoft.Extensions.Options;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.Search.Infrastructure.Shared;

/// <summary>
/// Adapter implementation for further education pupil search.
/// It delegates search execution to Azure Cognitive Search using domain-specific mappings and configuration.
/// </summary>
public sealed class AzureSearchServiceAdaptor<TResults, TDataTransferObject> : ISearchServiceAdapter<TResults, SearchFacets>
    where TDataTransferObject : class
{
    private readonly ISearchByKeywordService _searchByKeywordService;
    private readonly IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets> _facetsMapper;
    private readonly AzureSearchOptions _azureSearchOptions;
    private readonly ISearchOptionsBuilder _searchOptionsBuilder;
    private readonly IMapper<Pageable<SearchResult<TDataTransferObject>>, TResults> _dtoToOutputModelMapper;

    /// <summary>
    /// Constructs a new adapter for executing further education search queries.
    /// </summary>
    /// <param name="searchByKeywordService">Service that abstracts Azure Cognitive Search keyword-based querying.</param>
    /// <param name="azureSearchOptions">Search configuration options including index name, result size, and behavior flags.</param>
    /// <param name="searchResultMapper">Mapper to convert raw search results into domain-specific pupil search results.</param>
    /// <param name="facetsMapper">Mapper to convert facet results into structured domain-specific facet data.</param>
    /// <param name="searchOptionsBuilder">Builder for constructing query parameters passed into Azure Search.</param>
    public AzureSearchServiceAdaptor(
        ISearchByKeywordService searchByKeywordService,
        IOptions<AzureSearchOptions> azureSearchOptions,
        IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets> facetsMapper,
        ISearchOptionsBuilder searchOptionsBuilder,
        IMapper<Pageable<SearchResult<TDataTransferObject>>, TResults> dtoToOutputModelMapper)
    {
        ArgumentNullException.ThrowIfNull(azureSearchOptions);
        ArgumentNullException.ThrowIfNull(azureSearchOptions.Value);
        _azureSearchOptions = azureSearchOptions.Value;

        ArgumentNullException.ThrowIfNull(searchByKeywordService);
        _searchByKeywordService = searchByKeywordService;

        ArgumentNullException.ThrowIfNull(facetsMapper);
        _facetsMapper = facetsMapper;

        ArgumentNullException.ThrowIfNull(searchOptionsBuilder);
        _searchOptionsBuilder = searchOptionsBuilder;

        ArgumentNullException.ThrowIfNull(dtoToOutputModelMapper);
        _dtoToOutputModelMapper = dtoToOutputModelMapper;
    }

    /// <summary>
    /// Executes a pupil search using Azure Cognitive Search based on keyword input and configured criteria.
    /// </summary>
    /// <param name="searchServiceAdapterRequest">
    /// The adapter request containing search keyword, filter expressions, offset, search fields and facets.
    /// </param>
    /// <returns>
    /// Structured results including matched pupils, facet counts, and total number of hits.
    /// </returns>
    /// <exception cref="ApplicationException">
    /// Thrown when the Azure Search service fails to return valid results.
    /// </exception>
    public async Task<SearchResults<TResults, SearchFacets>> SearchAsync(SearchServiceAdapterRequest searchServiceAdapterRequest)
    {
        AzureSearchIndexOptions indexOptions = _azureSearchOptions.GetIndexOptions(searchServiceAdapterRequest.SearchIndexKey);

        SearchOptions searchOptions =
            _searchOptionsBuilder
                .WithSearchMode((SearchMode)indexOptions.SearchMode)
                .WithSize(indexOptions.Size)
                .WithOffset(searchServiceAdapterRequest.Offset)
                .WithIncludeTotalCount(indexOptions.IncludeTotalCount)
                .WithSearchFields(searchServiceAdapterRequest.SearchFields)
                .WithFacets(searchServiceAdapterRequest.Facets)
                .WithFilters(searchServiceAdapterRequest.SearchFilterRequests)
                .WithSortOrder(searchServiceAdapterRequest.SortOrdering)
        .Build();

        Response<SearchResults<TDataTransferObject>> searchResults =
            await _searchByKeywordService.SearchAsync<TDataTransferObject>(
                searchServiceAdapterRequest.SearchKeyword,
                indexOptions.SearchIndex,
                searchOptions) ?? throw new InvalidOperationException(
                        $"Unable to derive search results based on input {searchServiceAdapterRequest.SearchKeyword}.");

        return new SearchResults<TResults, SearchFacets>
        {
            Results = _dtoToOutputModelMapper.Map(searchResults.Value.GetResults()),
            FacetResults = searchResults.Value.Facets != null
                ? _facetsMapper.Map(searchResults.Value.Facets.ToDictionary()) : null
        };
    }
}
