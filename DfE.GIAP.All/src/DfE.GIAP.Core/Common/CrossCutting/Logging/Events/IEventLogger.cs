namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

public interface IEventLogger
{
    void LogSearch(bool isCustomSearch, string customTextSearch);
    void LogDownload(DownloadType downloadType, DownloadFileFormat downloadFormat, DownloadEventType? downloadEventType = null);
    void LogSignin(string userId, string sessionId, string orgUrn, string orgName, string orgCategory);
}

public class EventLogger : IEventLogger
{
    private readonly IBusinessEventFactory _businessEventFactory;
    private readonly IEnumerable<IEventSink> _sinks;

    public EventLogger(
        IBusinessEventFactory businessEventFactory,
        IEnumerable<IEventSink> sinks)
    {
        _businessEventFactory = businessEventFactory;
        _sinks = sinks;
    }

    public void LogSearch(bool isCustomSearch, string customTextSearch)
    {
        SearchEvent evt = _businessEventFactory.CreateSearch(isCustomSearch, customTextSearch);
        Dispatch(evt);
    }

    public void LogDownload(DownloadType downloadType, DownloadFileFormat downloadFormat, DownloadEventType? downloadEventType = null)
    {
        DownloadEvent evt = _businessEventFactory.CreateDownload(downloadType, downloadFormat, downloadEventType);
        Dispatch(evt);
    }

    public void LogSignin(string userId, string sessionId, string orgUrn, string orgName, string orgCategory)
    {
        SigninEvent evt = _businessEventFactory.CreateSignin(userId, sessionId, orgUrn, orgName, orgCategory);
        Dispatch(evt);
    }

    private void Dispatch(BusinessEvent evt)
    {
        foreach (IEventSink sink in _sinks)
        {
            sink.Write(evt);
        }
    }
}
