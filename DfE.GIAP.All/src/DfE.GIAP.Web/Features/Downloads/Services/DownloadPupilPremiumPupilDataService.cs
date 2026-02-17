using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DownloadOperationType = DfE.GIAP.Core.Common.CrossCutting.Logging.Events.DownloadOperationType;

namespace DfE.GIAP.Web.Features.Downloads.Services;

internal sealed class DownloadPupilPremiumPupilDataService : IDownloadPupilPremiumPupilDataService
{
    private readonly IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> _downloadDataUseCase;
    private readonly IEventLogger _eventLogger;

    public DownloadPupilPremiumPupilDataService(
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
        DownloadOperationType downloadEventType,
        CancellationToken ctx = default)
    {
        DownloadPupilDataRequest request = new(
          SelectedPupils: pupilUpns.Where(t => !string.IsNullOrWhiteSpace(t)),
          SelectedDatasets: [Core.Downloads.Application.Enums.Dataset.PP],
          DownloadType: Core.Downloads.Application.Enums.PupilDownloadType.PupilPremium,
          FileFormat: FileFormat.Csv);

        DownloadPupilDataResponse response = await _downloadDataUseCase.HandleRequestAsync(request);

        _eventLogger.LogDownload(
            downloadEventType,
            DownloadFileFormat.CSV,
            DownloadEventType.PP);

        return new DownloadPupilPremiumFilesResponse(response);
    }
}
