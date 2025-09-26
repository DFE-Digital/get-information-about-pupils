using DfE.GIAP.Core.Common.CrossCutting.Logging.Configuration;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Handlers;

public class TraceLogHandler : ITraceLogHandler
{
    private readonly IDictionary<string, SinkConfig> _configs;
    private readonly IDictionary<string, ITraceLogSink> _activeSinks;

    public TraceLogHandler(
        IOptions<LoggingOptions> options,
        IEnumerable<ITraceLogSink> sinks)
    {
        _configs = options.Value.Trace.Sinks;
        _activeSinks = sinks
                .Where(s => _configs.TryGetValue(s.Name, out SinkConfig? config) && config.Enabled)
                .ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    }

    public void Handle(Log<TracePayload> logEntry)
    {
        foreach (KeyValuePair<string, SinkConfig> kvp in _configs)
        {
            if (_activeSinks.TryGetValue(kvp.Key, out ITraceLogSink? sink) &&
                kvp.Value.AcceptedLogLevels.Contains(logEntry.Payload.Level))
            {
                sink.Log(logEntry);
            }
        }
    }
}
