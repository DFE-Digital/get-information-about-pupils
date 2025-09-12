using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;
public interface ILogRouter
{
    void Route(LogEntry entry);
}

public class LogRouter : ILogRouter
{
    private readonly IDictionary<string, SinkConfig> _sinkConfigs;
    private readonly IDictionary<string, ILogSink> _sinkInstances;

    public LogRouter(IOptions<LoggingOptions> options, IEnumerable<ILogSink> sinks)
    {
        _sinkConfigs = options.Value.Sinks;
        _sinkInstances = sinks.ToDictionary(s => s.GetType().Name.Replace("Sink", ""), StringComparer.OrdinalIgnoreCase);
    }

    public void Route(LogEntry entry)
    {
        foreach (KeyValuePair<string, SinkConfig> kvp in _sinkConfigs)
        {
            string sinkName = kvp.Key;
            SinkConfig config = kvp.Value;

            if (_sinkInstances.TryGetValue(sinkName, out ILogSink? sink) &&
                config.AcceptedLogLevels.Contains(entry.Level))
            {
                sink.Log(entry);
            }
        }
    }
}
