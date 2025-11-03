using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Handlers;

public class EventLogHandler : IEventLogHandler
{
    private readonly IEnumerable<IEventLogSink> _sinks;

    public EventLogHandler(IEnumerable<IEventLogSink> sinks)
    {
        _sinks = sinks;
    }

    public void Handle(Log<BusinessEventPayload> logEntry)
    {
        foreach (IEventLogSink sink in _sinks)
        {
            sink.Log(logEntry);
        }
    }
}
