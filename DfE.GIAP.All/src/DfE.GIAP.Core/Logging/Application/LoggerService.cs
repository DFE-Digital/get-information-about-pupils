using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using DfE.GIAP.Core.Logging.Application.Handlers;
using DfE.GIAP.Core.Logging.Application.Models;

namespace DfE.GIAP.Core.Logging.Application;

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
        TracePayloadOptions options = new()
        {
            Level = level,
            Message = message,
            Exception = exception,
            Category = category,
            Source = source,
            Context = context
        };

        LogEntry<TracePayload> logEntry = _logFactory.CreateLogEntry<TracePayload>(options);

        foreach (ITraceLogHandler handler in _traceLogHandlers)
        {
            handler.Handle(logEntry);
        }
    }
}
