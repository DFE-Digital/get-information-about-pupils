using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Infrastructure.Builders;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Options;
using Microsoft.Extensions.Options;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;

namespace DfE.GIAP.Core.Search.Infrastructure;

/// <summary>
/// Adapter implementation for further education pupil search.
/// It delegates search execution to Azure Cognitive Search using domain-specific mappings and configuration.
/// </summary>
public sealed class AzureSearchServiceAdapter : ISearchServiceAdapter<Learners, SearchFacets>
{
    private readonly ISearchByKeywordService _searchByKeywordService;
    private readonly IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners> _searchResultMapper;
    private readonly IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets> _facetsMapper;
    private readonly AzureSearchOptions _azureSearchOptions;
    private readonly ISearchOptionsBuilder _searchOptionsBuilder;

    /// <summary>
    /// Constructs a new adapter for executing further education search queries.
    /// </summary>
    /// <param name="searchByKeywordService">Service that abstracts Azure Cognitive Search keyword-based querying.</param>
    /// <param name="azureSearchOptions">Search configuration options including index name, result size, and behavior flags.</param>
    /// <param name="searchResultMapper">Mapper to convert raw search results into domain-specific pupil search results.</param>
    /// <param name="facetsMapper">Mapper to convert facet results into structured domain-specific facet data.</param>
    /// <param name="searchOptionsBuilder">Builder for constructing query parameters passed into Azure Search.</param>
    public AzureSearchServiceAdapter(
        ISearchByKeywordService searchByKeywordService,
        IOptions<AzureSearchOptions> azureSearchOptions,
        IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners> searchResultMapper,
        IMapper<Dictionary<string, IList<AzureFacetResult>>, SearchFacets> facetsMapper,
        ISearchOptionsBuilder searchOptionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(azureSearchOptions?.Value);
        _azureSearchOptions = azureSearchOptions.Value;
        _searchByKeywordService = searchByKeywordService;
        _searchResultMapper = searchResultMapper;
        _facetsMapper = facetsMapper;
        _searchOptionsBuilder = searchOptionsBuilder;
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
    public async Task<SearchResults<Learners, SearchFacets>> SearchAsync(
        SearchServiceAdapterRequest searchServiceAdapterRequest)
    {
        SearchOptions searchOptions =
            _searchOptionsBuilder
                .WithSearchMode((SearchMode)_azureSearchOptions.SearchMode)
                .WithSize(_azureSearchOptions.Size)
                .WithOffset(searchServiceAdapterRequest.Offset)
                .WithIncludeTotalCount(_azureSearchOptions.IncludeTotalCount)
                .WithSearchFields(searchServiceAdapterRequest.SearchFields)
                .WithFacets(searchServiceAdapterRequest.Facets)
                .WithFilters(searchServiceAdapterRequest.SearchFilterRequests)
                .WithSortOrder(searchServiceAdapterRequest.SortOrdering)
                .Build();

        Response<SearchResults<LearnerDataTransferObject>> searchResults =
            await _searchByKeywordService.SearchAsync<LearnerDataTransferObject>(
                searchServiceAdapterRequest.SearchKeyword,
                _azureSearchOptions.SearchIndex,
                searchOptions
            ).ConfigureAwait(false)
            ?? throw new ApplicationException(
                $"Unable to derive search results based on input {searchServiceAdapterRequest.SearchKeyword}.");

        return new SearchResults<Learners, SearchFacets>
        {
            Results = _searchResultMapper.Map(searchResults.Value.GetResults()),
            FacetResults = searchResults.Value.Facets != null
                ? _facetsMapper.Map(searchResults.Value.Facets.ToDictionary())
                : null,
            TotalNumberOfRecords = searchResults.Value.TotalCount
        };
    }
}
