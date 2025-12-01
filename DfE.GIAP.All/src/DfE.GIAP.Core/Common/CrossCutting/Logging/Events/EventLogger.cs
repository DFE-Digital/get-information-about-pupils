using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Sinks;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

public class EventLogger : IEventLogger
{
    private readonly IBusinessEventFactory _businessEventFactory;
    private readonly IEnumerable<IEventSink> _sinks;

    public EventLogger(
        IBusinessEventFactory businessEventFactory,
        IEnumerable<IEventSink> sinks)
    {
        ArgumentNullException.ThrowIfNull(businessEventFactory);
        ArgumentNullException.ThrowIfNull(sinks);
        _businessEventFactory = businessEventFactory;
        _sinks = sinks;
    }

    public void LogSearch(SearchIdentifierType searchIdentifierType, bool isCustomSearch, Dictionary<string, bool> filterFlags)
    {
        SearchEvent evt = _businessEventFactory.CreateSearch(searchIdentifierType, isCustomSearch, filterFlags);
        Dispatch(evt);
    }

    public void LogDownload(DownloadType downloadType, DownloadFileFormat downloadFormat,
        DownloadEventType? downloadEventType = null, string? batchId = null, Dataset? dataset = null)
    {
        DownloadEvent evt = _businessEventFactory.CreateDownload(downloadType, downloadFormat, downloadEventType, batchId, dataset);
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
            sink.Log(evt);
        }
    }
}
