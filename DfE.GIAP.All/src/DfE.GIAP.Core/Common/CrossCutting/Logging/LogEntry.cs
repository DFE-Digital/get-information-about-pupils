namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public static class LogEntryFactory
{
    public static LogEntry CreateTrace(
        LogLevel level,
        string message,
        string? category = null,
        Exception? exception = null,
        object? context = null) =>
        new(level, message, exception, context, DateTime.UtcNow, LogType.Trace, category);

    public static LogEntry CreateBusinessEvent(
        string message,
        string? category = null,
        object? context = null) =>
        new(LogLevel.Information, message, null, context, DateTime.UtcNow, LogType.BusinessEvent, category);
}

public record LogEntry(
    LogLevel Level,
    string Message,
    Exception? Exception,
    object? Context,
    DateTime Timestamp,
    LogType Type,
    string? Category = null);
