using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DownloadType = DfE.GIAP.Core.Common.CrossCutting.Logging.Events.DownloadType;

namespace DfE.GIAP.Web.Features.Downloads.Services;

internal sealed class DownloadPupilPremiumDataForPupilsService : IDownloadPupilPremiumDataForPupilsService
{
    private readonly IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> _downloadDataUseCase;
    private readonly IEventLogger _eventLogger;

    public DownloadPupilPremiumDataForPupilsService(
        IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> downloadDataUseCase,
        IEventLogger eventLogger)
    {
        ArgumentNullException.ThrowIfNull(downloadDataUseCase);
        _downloadDataUseCase = downloadDataUseCase;

        ArgumentNullException.ThrowIfNull(eventLogger);
        _eventLogger = eventLogger;
    }

    public async Task<DownloadPupilPremiumFilesResponse> DownloadAsync(
        IEnumerable<string> pupilUpns,
        DownloadType downloadEventType,
        CancellationToken ctx = default)
    {
        DownloadPupilDataRequest request = new(
          SelectedPupils: pupilUpns.Where(t => !string.IsNullOrWhiteSpace(t)),
          SelectedDatasets: [Core.Downloads.Application.Enums.Dataset.PP],
          DownloadType: Core.Downloads.Application.Enums.DownloadType.PupilPremium,
          FileFormat: FileFormat.Csv);

        DownloadPupilDataResponse response = await _downloadDataUseCase.HandleRequestAsync(request);

        _eventLogger.LogDownload(
            downloadEventType,
            DownloadFileFormat.CSV,
            DownloadEventType.PP);

        return new DownloadPupilPremiumFilesResponse(response);
    }
}
