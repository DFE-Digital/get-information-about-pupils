using DownloadType = DfE.GIAP.Core.Common.CrossCutting.Logging.Events.DownloadType;

namespace DfE.GIAP.Web.Features.Downloads.Services;

public interface IDownloadPupilPremiumDataForPupilsService
{
    Task<DownloadPupilPremiumFilesResponse> DownloadAsync(
        IEnumerable<string> pupilUpns,
        DownloadType downloadEventType,
        CancellationToken ctx = default);
}
