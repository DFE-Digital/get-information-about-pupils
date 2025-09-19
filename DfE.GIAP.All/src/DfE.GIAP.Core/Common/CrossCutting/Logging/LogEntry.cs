namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public static class LogEntryFactory
{
    public static LogEntry<TracePayload> CreateWithTracePayload(
        LogLevel level,
        string message,
        Exception? exception = null,
        string? category = null,
        string? source = null,
        Dictionary<string, object>? context = null,
        string? correlationId = null,
        string? userId = null,
        string? sessionId = null)
    {
        TracePayload payload = new(
            Level: level,
            Message: message,
            Exception: exception,
            Category: category,
            Source: source,
            Context: context);

        return new LogEntry<TracePayload>
        {
            Timestamp = DateTime.UtcNow,
            CorrelationId = correlationId,
            UserID = userId,
            SessionId = sessionId,
            Payload = payload
        };
    }

}

public enum LogLevel { Debug, Information, Error }

public record LogEntry<TPayload>
{
    public string? CorrelationId { get; init; }
    public string? UserID { get; init; }
    public string? SessionId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public required TPayload Payload { get; init; }
}

public record TracePayload(
    string Message,
    LogLevel Level = LogLevel.Information,
    Exception? Exception = null,
    string? Category = null,
    string? Source = null,
    Dictionary<string, object>? Context = null);

