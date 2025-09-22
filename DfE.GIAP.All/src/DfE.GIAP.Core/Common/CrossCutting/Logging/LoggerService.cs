using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public class LoggerService : ILoggerService
{
    private readonly IEnumerable<ITraceLogHandler> _traceLogHandlers;
    private readonly ILogEntryFactory _logFactory;

    public LoggerService(
        IEnumerable<ITraceLogHandler> traceLogHandlers,
        ILogEntryFactory logFactory)
    {
        ArgumentNullException.ThrowIfNull(traceLogHandlers);
        ArgumentNullException.ThrowIfNull(logFactory);
        _traceLogHandlers = traceLogHandlers;
        _logFactory = logFactory;
    }

    public void LogTrace(
         LogLevel level,
         string message,
         Exception? exception = null,
         string? category = null,
         string? source = null,
         Dictionary<string, object>? context = null)
    {
        LogEntry<TracePayload> logEntry = _logFactory.CreateTraceLogEntry(
            level: level,
            message: message,
            exception: exception,
            category: category,
            source: source,
            context: context);

        foreach (ITraceLogHandler handler in _traceLogHandlers)
        {
            handler.Handle(logEntry);
        }
    }
}
