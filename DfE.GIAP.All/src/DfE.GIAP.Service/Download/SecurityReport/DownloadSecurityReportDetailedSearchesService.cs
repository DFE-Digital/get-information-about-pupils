using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Helpers;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Service.Download.SecurityReport
{
    public class DownloadSecurityReportDetailedSearchesService : IDownloadSecurityReportDetailedSearchesService
    {
        private readonly IApiService _apiProcessorService;
        private AzureAppSettings _azureAppSettings;

        public DownloadSecurityReportDetailedSearchesService(IApiService apiProcessorService, IOptions<AzureAppSettings> azureAppSettings)
        {
            _apiProcessorService = apiProcessorService;
            _azureAppSettings = azureAppSettings.Value;
        }

        public async Task<ReturnFile> GetSecurityReportDetailedSearches(string searchParameter, SecurityReportSearchType searchType,
            AzureFunctionHeaderDetails azureFunctionHeaderDetails, bool isFe)
        {
            var requestBody = new SecurityReportRequestBody()
            {
                SearchType = searchType.ToString(),
                SearchParameter = searchParameter,
                IsFe = isFe
            };

            var response = await _apiProcessorService.PostAsync<SecurityReportRequestBody, ReturnFile>(_azureAppSettings.DownloadSecurityReportDetailedSearchesUrl.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
        }
    }
}
