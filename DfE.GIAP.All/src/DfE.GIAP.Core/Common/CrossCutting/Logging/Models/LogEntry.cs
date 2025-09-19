namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

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

