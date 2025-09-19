using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public class LoggerService : ILoggerService
{
    private readonly IEnumerable<ITraceLogHandler> _traceLogHandlers;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public LoggerService(
        IEnumerable<ITraceLogHandler> traceLogHandlers,
        ICorrelationContextAccessor correlationContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(traceLogHandlers);
        ArgumentNullException.ThrowIfNull(correlationContextAccessor);
        _traceLogHandlers = traceLogHandlers;
        _correlationContextAccessor = correlationContextAccessor;
    }

    public void LogTrace(
         LogLevel level,
         string message,
         Exception? exception = null,
         string? category = null,
         string? source = null,
         Dictionary<string, object>? context = null)
    {
        LogEntry<TracePayload> entry = LogEntryFactory.CreateWithTracePayload(
            level: level,
            message: message,
            exception: exception,
            category: category,
            source: source,
            context: context,
            correlationId: _correlationContextAccessor.CorrelationId,
            userId: _correlationContextAccessor.UserId,
            sessionId: _correlationContextAccessor.SessionId);

        foreach (ITraceLogHandler handler in _traceLogHandlers)
        {
            handler.Handle(entry);
        }
    }
}
