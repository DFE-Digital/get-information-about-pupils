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
            Message: message,
            Exception: exception,
            Category: category,
            Source: source,
            Context: context);

        return new LogEntry<TracePayload>
        {
            Level = level,
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
            Data: data);

        return new LogEntry<AuditPayload>
        {
            Level = LogLevel.Information,
            Timestamp = DateTime.UtcNow,
            CorrelationId = correlationId,
            Payload = payload
        };
    }
}


public record LogEntry<TPayload>
{
    public LogLevel Level { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public required TPayload Payload { get; init; }
}

public record TracePayload(
    string Message,
    Exception? Exception = null,
    string? Category = null,
    string? Source = null,
    Dictionary<string, object>? Context = null);

public record AuditPayload(
    string EventName,
    string? Category = null,
    string? Source = null,
    Dictionary<string, object>? Data = null);

public enum LogLevel { Verbose, Debug, Information, Warning, Error, Critical }
