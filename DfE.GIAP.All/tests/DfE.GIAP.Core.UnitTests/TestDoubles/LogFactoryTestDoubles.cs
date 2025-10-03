using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
public static class LogFactoryTestDoubles
{
    public static Log<TracePayload> CreateDefaultTraceLog(
        string message = "message",
        LogLevel level = LogLevel.Information,
        string correlationId = "correlationId",
        string userId = "userId",
        string sessionId = "sessionId",
        Exception? exception = null,
        string? category = "category",
        string? source = "source",
        Dictionary<string, object>? context = null)
    {
        TracePayload payload = new(
            Message: message,
            CorrelationId: correlationId,
            UserID: userId,
            SessionId: sessionId,
            Level: level,
            Exception: exception,
            Category: category,
            Source: source,
            Context: context ?? new Dictionary<string, object>()
        );

        return new Log<TracePayload>
        {
            Payload = payload
        };
    }
}
