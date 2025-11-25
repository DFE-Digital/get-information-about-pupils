namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

public interface IBusinessEventFactory
{
    DownloadEvent CreateDownload(DownloadType downloadType, DownloadFileFormat downloadFormat, DownloadEventType? downloadEventType = null);
    SearchEvent CreateSearch(bool isCustomSearch, string customTextSearch);
    SigninEvent CreateSignin(string userId, string sessionId, string orgUrn, string orgName, string orgCategory);
}

