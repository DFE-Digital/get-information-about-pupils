using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.Download;
using DfE.GIAP.Web.Config;
using DfE.GIAP.Web.Services.ApiProcessor;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Services.Download.CTF;

public class DownloadCommonTransferFileService : IDownloadCommonTransferFileService
{
    private readonly IApiService _apiProcessorService;
    private readonly AzureAppSettings _azureAppSettings;
    private readonly IEventLogger _eventLogger;

    public DownloadCommonTransferFileService(
        IApiService apiProcessorService,
        IOptions<AzureAppSettings> azureFunctionUrls,
        IEventLogger eventLogger)
    {
        _apiProcessorService = apiProcessorService;
        _azureAppSettings = azureFunctionUrls.Value;
        _eventLogger = eventLogger;
    }

    public async Task<ReturnFile> GetCommonTransferFile(string[] upns,
                                                        string[] sortOrder,
                                                        string localAuthorityNumber,
                                                        string establishmentNumber,
                                                        bool isOrganisationEstablishment,
                                                        AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                                        ReturnRoute returnRoute)
    {
        var getCTFFile = _azureAppSettings.DownloadCommonTransferFileUrl;
        var requestBody = new CommonTransferFile
        {
            UPNs = upns,
            EstablishmentNumber = establishmentNumber,
            LocalAuthorityNumber = localAuthorityNumber,
            IsEstablishment = isOrganisationEstablishment,
            SortOrder = sortOrder
        };

        ReturnFile response = await _apiProcessorService.PostAsync<CommonTransferFile, ReturnFile>(new Uri(getCTFFile),
                                                                                            requestBody,
                                                                                            azureFunctionHeaderDetails)
                                                 .ConfigureAwait(false);

        _eventLogger.LogDownload(Core.Common.CrossCutting.Logging.Events.DownloadType.Search, DownloadFileFormat.XML, DownloadEventType.CTF);

        return response;
    }
}
