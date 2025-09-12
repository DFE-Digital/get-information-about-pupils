using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILogEventHandler
{
    void Handle(LogEntry logEntry);
}

public class TraceLogRouter : ILogEventHandler
{
    private readonly IDictionary<string, SinkConfig> _configs;
    private readonly IDictionary<string, ITraceSink> _activeSinks;

    public TraceLogRouter(IOptions<LoggingOptions> options, IEnumerable<ITraceSink> sinks)
    {
        _configs = options.Value.Trace.Sinks;
        _activeSinks = sinks.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    }

    public void Handle(LogEntry logEntry)
    {
        if (logEntry.Type is not LogType.Trace) return;

        Route(logEntry);
    }

    private void Route(LogEntry entry)
    {
        foreach (KeyValuePair<string, SinkConfig> kvp in _configs)
        {
            if (_activeSinks.TryGetValue(kvp.Key, out ITraceSink? sink) &&
                kvp.Value.AcceptedLogLevels.Contains(entry.Level))
            {
                sink.Log(entry);
            }
        }
    }
}

public class BusinessEventRouter : ILogEventHandler
{
    private readonly List<IBusinessEventSink> _activeSinks;

    public BusinessEventRouter(IOptions<LoggingOptions> options, IEnumerable<IBusinessEventSink> sinks)
    {
        List<string> configSinks = options.Value.BusinessEvents.Sinks;
        _activeSinks = sinks.Where(s => configSinks.Contains(s.Name, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    public void Handle(LogEntry logEntry)
    {
        if (logEntry.Type is not LogType.BusinessEvent) return;

        Route(logEntry);
    }

    private void Route(LogEntry entry)
    {
        foreach (ILogSink sink in _activeSinks)
        {
            sink.Log(entry);
        }
    }
}
