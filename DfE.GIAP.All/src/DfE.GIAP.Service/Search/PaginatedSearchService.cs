using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.ApiProcessor;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Service.Search;

public class PaginatedSearchService : IPaginatedSearchService
{
    private readonly IApiService _apiProcessorService;
    private AzureAppSettings _azureAppSettings;
    private readonly IEventLogger _eventLogger;

    public PaginatedSearchService(
        IApiService apiProcessorService,
        IOptions<AzureAppSettings> azureFunctionUrls,
        IEventLogger eventLogger)
    {
        _apiProcessorService = apiProcessorService;
        _azureAppSettings = azureFunctionUrls.Value;
        _eventLogger = eventLogger;
    }

    /// <summary>
    /// Fetches a page of learners from the FA REST API
    /// </summary>
    /// <param name="searchText">name or learner numbers</param>
    /// <param name="filters">key/value pairs of filters. Key being a filter type and value being an array of strings (such as names)</param>
    /// <param name="pageSize">number of rows per page</param>
    /// <param name="pageNumber">page number wanted</param>
    /// <param name="indexType">type of index, NPD, pupil premium or FE</param>
    /// <param name="queryType">learner number search or text based search</param>
    /// <param name="azureFunctionHeaderDetails">headers for client and session IDI</param>
    /// <param name="sortField">optional field to sort against</param>
    /// <param name="sortDirection">optional direction if sorting. If you give a sort field you must give a sort direction</param>
    /// <returns></returns>
    public async Task<PaginatedResponse> GetPage(
        string searchText,
        Dictionary<string, string[]> filters,
        int pageSize,
        int pageNumber,
        AzureSearchIndexType indexType,
        AzureSearchQueryType queryType,
        AzureFunctionHeaderDetails azureFunctionHeaderDetails,
        string sortField = "",
        string sortDirection = "")
    {
        var request = new PaginatedSearchRequest()
        {
            SearchText = searchText,
            Filters = filters,
            PageSize = pageSize,
            PageNumber = pageNumber,
            SortField = sortField,
            SortDirection = sortDirection
        };

        var queryUrl = GetSearchUrl(indexType, queryType);
        var response = await _apiProcessorService.PostAsync<PaginatedSearchRequest, PaginatedResponse>(queryUrl.ConvertToUri(), request, azureFunctionHeaderDetails).ConfigureAwait(false);

        SearchIdentifierType searchIdentifierType = indexType is AzureSearchIndexType.FurtherEducation ? SearchIdentifierType.ULN : SearchIdentifierType.UPN;
        bool isCustomSearch = queryType is AzureSearchQueryType.Text;

        Dictionary<string, bool> flags = ConvertFiltersToFlags(filters);
        _eventLogger.LogSearch(searchIdentifierType, isCustomSearch, flags);

        return response;
    }

    public static Dictionary<string, bool> ConvertFiltersToFlags(Dictionary<string, string[]> filters)
    {
        Dictionary<string, bool> flags = new();

        if (filters is null)
            return flags;

        foreach (KeyValuePair<string, string[]> kvp in filters)
        {
            // True if the filter has at least one value
            flags[kvp.Key] = kvp.Value is not null && kvp.Value.Length > 0;
        }

        return flags;
    }

    private string GetSearchUrl(AzureSearchIndexType indexType, AzureSearchQueryType queryType)
    {
        return _azureAppSettings.PaginatedSearchUrl
            .Replace("{indexType}", indexType.ToString())
            .Replace("{queryType}", queryType.ToString());
    }
}
