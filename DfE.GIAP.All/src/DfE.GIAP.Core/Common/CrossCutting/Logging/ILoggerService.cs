using DfE.GIAP.Core.Common.Domain;

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
    private readonly ILogMediator _mediator;

    public LoggerService(ILogMediator mediator)
    {
        _mediator = mediator;
    }

    public void Log(LogLevel level, string message, Exception? ex = null)
    {
        LogEntry entry = new LogEntry(level, message, ex, Timestamp: DateTime.UtcNow);
        _mediator.Publish(entry);
    }
}
