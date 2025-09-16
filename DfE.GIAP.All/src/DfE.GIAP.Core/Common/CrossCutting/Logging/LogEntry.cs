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
        string? correlationId = null)
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
            Payload = payload
        };
    }

    public static LogEntry<AuditPayload> CreateWithAuditPayload(
        string eventName,
        string? category = null,
        string? source = null,
        Dictionary<string, object>? data = null,
        string? correlationId = null)
    {
        AuditPayload payload = new(
            EventName: eventName,
            Category: category,
            Source: source,
            Context: data);

        return new LogEntry<AuditPayload>
        {
            Timestamp = DateTime.UtcNow,
            CorrelationId = correlationId,
            Payload = payload
        };
    }
}


public record LogEntry<TPayload>
{
    public string? CorrelationId { get; init; }
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

public record AuditPayload(
    string EventName,
    string? Category = null,
    string? Source = null,
    Dictionary<string, object>? Context = null);

public enum LogLevel { Debug, Information, Warning, Error }
