using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.Download;
using DfE.GIAP.Service.ApiProcessor;
using Microsoft.Extensions.Options;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using Microsoft.Extensions.Hosting;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

namespace DfE.GIAP.Service.Download.CTF;

public class DownloadCommonTransferFileService : IDownloadCommonTransferFileService
{
    private readonly IApiService _apiProcessorService;
    private AzureAppSettings _azureAppSettings;
    private readonly IEventLogging _eventLogging;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IEventLogger _eventLogger;

    public DownloadCommonTransferFileService(
        IApiService apiProcessorService,
        IOptions<AzureAppSettings> azureFunctionUrls,
        IEventLogging eventLogging,
        IEventLogger eventLogger,
        IHostEnvironment hostEnvironment)
    {
        _apiProcessorService = apiProcessorService;
        _azureAppSettings = azureFunctionUrls.Value;
        _eventLogging = eventLogging;
        _eventLogger = eventLogger;
        _hostEnvironment = hostEnvironment;
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

        switch (returnRoute)
        {
            case ReturnRoute.NationalPupilDatabase:
                _eventLogging.TrackEvent(1106, $"NPD UPN CTF download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;

            case ReturnRoute.NonNationalPupilDatabase:
                _eventLogging.TrackEvent(1109, $"NPD non-UPN CTF download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;

            case ReturnRoute.MyPupilList:
                _eventLogging.TrackEvent(1116, $"MPL CTF download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;
        }

        var requestBody = new CommonTransferFile
        {
            UPNs = upns,
            EstablishmentNumber = establishmentNumber,
            LocalAuthorityNumber = localAuthorityNumber,
            IsEstablishment = isOrganisationEstablishment,
            SortOrder = sortOrder
        };
        var response = await _apiProcessorService.PostAsync<CommonTransferFile, ReturnFile>(getCTFFile.ConvertToUri(),
                                                                                            requestBody,
                                                                                            azureFunctionHeaderDetails)
                                                 .ConfigureAwait(false);

        _eventLogger.LogDownload(Core.Common.CrossCutting.Logging.Events.DownloadType.Search, DownloadFileFormat.XML, DownloadEventType.CTF);

        return response;
    }
}
