using DownloadOperationType = DfE.GIAP.Core.Common.CrossCutting.Logging.Events.DownloadOperationType;

namespace DfE.GIAP.Web.Features.Downloads.Services;

public interface IDownloadPupilPremiumPupilDataService
{
    Task<DownloadPupilPremiumFilesResponse> DownloadAsync(
        IEnumerable<string> pupilUpns,
        DownloadOperationType downloadEventType,
        CancellationToken ctx = default);
}
