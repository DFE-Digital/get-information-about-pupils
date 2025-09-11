namespace DfE.GIAP.Core.Common.Infrastructure.Logging;

public enum LogLevel { Verbose, Debug, Information, Warning, Error, Critical }
public record LogEntry(LogLevel Level, string Message, Exception? Exception = null, object? Context = null, DateTime? Timestamp = null);


public interface ILoggerService
{
    public void Log(LogLevel level, string message, Exception? ex = null);
}

public class LoggerService : ILoggerService
{
    private readonly IEnumerable<ILogSink> _sinks;

    public LoggerService(IEnumerable<ILogSink> sinks)
    {
        _sinks = sinks;
    }

    public void Log(LogLevel level, string message, Exception? ex = null)
    {
        LogEntry logEntry = new(
            Level: level,
            Message: message,
            Exception: ex,
            Timestamp: DateTime.UtcNow);

        foreach (ILogSink sink in _sinks)
        {
            sink.Log(logEntry);
        }
    }
}
