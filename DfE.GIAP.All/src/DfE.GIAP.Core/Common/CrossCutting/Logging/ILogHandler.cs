using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILogHandler<TPayload>
{
    void Handle(LogEntry<TPayload> entry);
}

public interface ITraceLogHandler : ILogHandler<TracePayload> { }
public interface IAuditLogHandler : ILogHandler<AuditPayload> { }


public class TraceLogHandler : ITraceLogHandler
{
    private readonly IDictionary<string, SinkConfig> _configs;
    private readonly IDictionary<string, ITraceLogSink> _activeSinks;

    public TraceLogHandler(
        IOptions<LoggingOptions> options,
        IEnumerable<ITraceLogSink> sinks)
    {
        _configs = options.Value.Trace.Sinks;
        _activeSinks = sinks.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    }

    public void Handle(LogEntry<TracePayload> logEntry)
    {
        foreach (KeyValuePair<string, SinkConfig> kvp in _configs)
        {
            if (_activeSinks.TryGetValue(kvp.Key, out ITraceLogSink? sink) &&
                kvp.Value.AcceptedLogLevels.Contains(logEntry.Level))
            {
                sink.Log(logEntry);
            }
        }
    }
}

public class AuditLogHandler : IAuditLogHandler
{
    private readonly List<IAuditLogSink> _activeSinks;

    public AuditLogHandler(
        IOptions<LoggingOptions> options,
        IEnumerable<IAuditLogSink> sinks)
    {
        List<string> configSinks = options.Value.BusinessEvents.Sinks;
        _activeSinks = sinks.Where(s => configSinks.Contains(s.Name, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    public void Handle(LogEntry<AuditPayload> logEntry)
    {
        foreach (IAuditLogSink sink in _activeSinks)
        {
            sink.Log(logEntry);
        }
    }
}
