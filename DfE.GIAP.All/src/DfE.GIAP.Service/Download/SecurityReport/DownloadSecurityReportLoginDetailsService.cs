using System.Net.Http;
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
    public class DownloadSecurityReportLoginDetailsService : IDownloadSecurityReportLoginDetailsService
    {
        private readonly IApiService _apiProcessorService;
        private AzureAppSettings _azureAppSettings;

        public DownloadSecurityReportLoginDetailsService(IApiService apiProcessorService, IOptions<AzureAppSettings> azureAppSettings)
        {
            _apiProcessorService = apiProcessorService;
            _azureAppSettings = azureAppSettings.Value;
        }

        public async Task<ReturnFile> GetSecurityReportLoginDetails(string searchParameter, SecurityReportSearchType searchType, AzureFunctionHeaderDetails azureFunctionHeaderDetails)
        {
            var requestBody = new SecurityReportRequestBody()
            {
                SearchType = searchType.ToString(),
                SearchParameter = searchParameter
            };

            var response = await _apiProcessorService.PostAsync<SecurityReportRequestBody, ReturnFile>(_azureAppSettings.DownloadSecurityReportLoginDetailsUrl.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
        }
    }
}