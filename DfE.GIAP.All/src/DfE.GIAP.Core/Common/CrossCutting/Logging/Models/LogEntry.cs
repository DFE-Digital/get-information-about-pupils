namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

public record LogEntry<TPayload>
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public required TPayload Payload { get; init; }
}

public record TracePayload(
    string Message,
    string CorrelationId,
    string UserID,
    string SessionId,
    LogLevel Level = LogLevel.Information,
    Exception? Exception = null,
    string? Category = null,
    string? Source = null,
    Dictionary<string, object>? Context = null);



