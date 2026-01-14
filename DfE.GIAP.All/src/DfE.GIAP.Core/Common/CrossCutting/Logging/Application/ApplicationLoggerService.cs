using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Handlers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application;

public class ApplicationLoggerService : IApplicationLoggerService
{
    private readonly IEnumerable<ITraceLogHandler> _traceLogHandlers;
    private readonly IApplicationLogEntryFactory<TracePayloadOptions, TracePayload> _traceLogFactory;

    public ApplicationLoggerService(
        IEnumerable<ITraceLogHandler> traceLogHandlers,
        IApplicationLogEntryFactory<TracePayloadOptions, TracePayload> traceLogFactory)
    {
        ArgumentNullException.ThrowIfNull(traceLogHandlers);
        ArgumentNullException.ThrowIfNull(traceLogFactory);
        _traceLogHandlers = traceLogHandlers;
        _traceLogFactory = traceLogFactory;
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

        Log<TracePayload> logEntry = _traceLogFactory.Create(options);
        foreach (ITraceLogHandler handler in _traceLogHandlers)
            handler.Handle(logEntry);
    }
}
