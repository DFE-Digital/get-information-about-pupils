namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

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

