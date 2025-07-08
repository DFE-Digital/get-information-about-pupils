using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Helpers;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Download.SecurityReport
{
    public class DownloadSecurityReportByUpnUlnService : IDownloadSecurityReportByUpnUlnService
    {
        private readonly IApiService _apiProcessorService;
        private AzureAppSettings _azureAppSettings;

        public DownloadSecurityReportByUpnUlnService(
            IApiService apiProcessorService,
            IOptions<AzureAppSettings> azureFunctionUrls
            )
        {
            _apiProcessorService = apiProcessorService;
            _azureAppSettings = azureFunctionUrls.Value;
        }

        public async Task<ReturnFile> GetSecurityReportByUpn(string upn, AzureFunctionHeaderDetails azureFunctionHeaderDetails)
        {
            var downloadSecurityReportByUpnUrl = _azureAppSettings.DownloadSecurityReportByUpnUrl;

            var requestBody = new SecurityReportByUPNULNRequestBody { UPNULN = upn };
            var response = await _apiProcessorService.PostAsync<SecurityReportByUPNULNRequestBody, ReturnFile>(downloadSecurityReportByUpnUrl.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
        }

        public async Task<ReturnFile> GetSecurityReportByUln(string uln, AzureFunctionHeaderDetails azureFunctionHeaderDetails)
        {
            var downloadSecurityReportByUlnUrl = _azureAppSettings.DownloadSecurityReportByUlnUrl;

            var requestBody = new SecurityReportByUPNULNRequestBody { UPNULN = uln };
            var response = await _apiProcessorService.PostAsync<SecurityReportByUPNULNRequestBody, ReturnFile>(downloadSecurityReportByUlnUrl.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
        }
    }
}
