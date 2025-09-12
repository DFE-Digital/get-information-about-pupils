namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

// Domain/Application
public enum LogLevel { Verbose, Debug, Information, Warning, Error, Critical }
public record LogEntry(LogLevel Level, string Message, Exception? Exception = null, object? Context = null, DateTime? Timestamp = null);

// Application
public interface ILoggerService
{
    public void Log(LogLevel level, string message, Exception? ex = null);
}

// Application
public class LoggerService : ILoggerService
{
    // dispatch event -> subscribers -> router -> sinks
    private readonly ILogRouter _router;

    public LoggerService(ILogRouter router)
    {
        _router = router;
    }

    public void Log(LogLevel level, string message, Exception? ex = null)
    {
        LogEntry log = new(level, message, ex, Timestamp: DateTime.UtcNow);
        _router.Route(log);
    }
}
