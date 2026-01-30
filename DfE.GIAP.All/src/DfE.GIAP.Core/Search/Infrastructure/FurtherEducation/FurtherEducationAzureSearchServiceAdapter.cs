using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Shared;

namespace DfE.GIAP.Core.Search.Infrastructure.FurtherEducation;

/// <summary>
/// Adapter implementation for further education pupil search.
/// It delegates search execution to Azure Cognitive Search using domain-specific mappings and configuration.
/// </summary>
public sealed class FurtherEducationAzureSearchServiceAdapter : ISearchServiceAdapter<FurtherEducationLearners, SearchFacets>
{
    private readonly IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners> _searchResultMapper;
    private readonly IAzureSearchByKeywordService _azureSearchByKeywordService;

    /// <summary>
    /// Constructs a new adapter for executing further education search queries.
    /// </summary>
    /// <param name="searchByKeywordService">Service that abstracts Azure Cognitive Search keyword-based querying.</param>
    /// <param name="azureSearchOptions">Search configuration options including index name, result size, and behavior flags.</param>
    /// <param name="searchResultMapper">Mapper to convert raw search results into domain-specific pupil search results.</param>
    /// <param name="facetsMapper">Mapper to convert facet results into structured domain-specific facet data.</param>
    /// <param name="searchOptionsBuilder">Builder for constructing query parameters passed into Azure Search.</param>
    public FurtherEducationAzureSearchServiceAdapter(
        IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners> searchResultMapper,
        IAzureSearchByKeywordService azureSearchByKeywordService)
    {
        ArgumentNullException.ThrowIfNull(searchResultMapper);
        _searchResultMapper = searchResultMapper;

        ArgumentNullException.ThrowIfNull(azureSearchByKeywordService);
        _azureSearchByKeywordService = azureSearchByKeywordService;
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
    public Task<SearchResults<FurtherEducationLearners, SearchFacets>> SearchAsync(
        SearchServiceAdapterRequest searchServiceAdapterRequest)
    {
        return
            _azureSearchByKeywordService.SearchByKeywordAsync(
                searchServiceAdapterRequest,
                _searchResultMapper);
    }
}
