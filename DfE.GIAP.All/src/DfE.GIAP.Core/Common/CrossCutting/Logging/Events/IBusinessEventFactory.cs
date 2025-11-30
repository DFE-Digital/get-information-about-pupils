using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

public interface IBusinessEventFactory
{
    DownloadEvent CreateDownload(DownloadType downloadType, DownloadFileFormat downloadFormat,
        DownloadEventType? downloadEventType = null, string? batchId = null, Dataset? dataset = null);
    SearchEvent CreateSearch(SearchIdentifierType searchIdentifierType, bool isCustomSearch, Dictionary<string, bool> filterFlags);
    SigninEvent CreateSignin(string userId, string sessionId, string orgUrn, string orgName, string orgCategory);
}
