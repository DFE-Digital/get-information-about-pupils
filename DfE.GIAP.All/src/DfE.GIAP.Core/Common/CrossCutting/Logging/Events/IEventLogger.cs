namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

public interface IEventLogger
{
    void LogSearch(SearchIdentifierType searchIdentifierType, bool isCustomSearch, Dictionary<string, bool> filterFlags);
    void LogDownload(DownloadType downloadType, DownloadFileFormat downloadFormat,
        DownloadEventType? downloadEventType = null, string? batchId = null, Dataset? dataset = null);
    void LogSignin(string userId, string sessionId, string orgUrn, string orgName, string orgCategory);
}
