using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.ApiProcessor;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Search
{
    public class PaginatedSearchService : IPaginatedSearchService
    {
        private readonly IApiService _apiProcessorService;
        private AzureAppSettings _azureAppSettings;

        public PaginatedSearchService(
            IApiService apiProcessorService,
            IOptions<AzureAppSettings> azureFunctionUrls)
        {
            _apiProcessorService = apiProcessorService;
            _azureAppSettings = azureFunctionUrls.Value;
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

            return response;
        }

        private string GetSearchUrl(AzureSearchIndexType indexType, AzureSearchQueryType queryType)
        {
            return _azureAppSettings.PaginatedSearchUrl
                .Replace("{indexType}", indexType.ToString())
                .Replace("{queryType}", queryType.ToString());
        }
    }
}
