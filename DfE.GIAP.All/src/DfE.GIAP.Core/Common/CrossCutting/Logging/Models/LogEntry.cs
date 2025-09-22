namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

public record LogEntry<TPayload>
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required TPayload Payload { get; init; }
}
