namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILoggerService
{
    void LogTrace(LogLevel level, string message, string? category = null, Exception? ex = null, object? context = null);
    void LogBusinessEvent(string message, string? category = null, object? context = null);
}

// Application
public class LoggerService : ILoggerService
{
    private readonly ILogMediator _mediator;

    public LoggerService(ILogMediator mediator)
    {
        _mediator = mediator;
    }

    public void LogTrace(LogLevel level, string message, string? category = null, Exception? ex = null, object? context = null)
    {
        LogEntry entry = LogEntryFactory.CreateTrace(level, message, category, ex, context);
        _mediator.Publish(entry);
    }

    public void LogBusinessEvent(string message, string? category = null, object? context = null)
    {
        LogEntry entry = LogEntryFactory.CreateBusinessEvent(message, category, context);
        _mediator.Publish(entry);
    }
}
