using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILogRouter
{
    void Route(LogEntry entry);
}

public class LogRouter : ILogRouter
{
    private readonly IDictionary<string, SinkConfig> _configs;
    private readonly IDictionary<string, ILogSink> _sinks;

    public LogRouter(IOptions<LoggingOptions> options, IEnumerable<ILogSink> sinks)
    {
        _configs = options.Value.Sinks;
        _sinks = sinks.ToDictionary(s => s.GetType().Name.Replace("Sink", ""), StringComparer.OrdinalIgnoreCase);
    }

    public void Route(LogEntry entry)
    {
        foreach (KeyValuePair<string, SinkConfig> kvp in _configs)
        {
            if (_sinks.TryGetValue(kvp.Key, out ILogSink? sink) &&
                kvp.Value.AcceptedLogLevels.Contains(entry.Level))
            {
                sink.Log(entry);
            }
        }
    }
}
