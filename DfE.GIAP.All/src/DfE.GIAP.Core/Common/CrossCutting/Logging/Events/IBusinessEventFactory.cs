namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

public interface IBusinessEventFactory
{
    SigninEvent CreateSignin();
    DownloadEvent CreateDownload(DownloadType downloadType, DownloadFileFormat downloadFormat, DownloadEventType? downloadEventType = null);
    SearchEvent CreateSearch(bool isCustomSearch, string customTextSearch);
}

