namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILoggerService
{
    void LogTrace(LogLevel level, string message, Exception? exception = null, string? category = null, string? source = null, Dictionary<string, object>? context = null, string? correlationId = null);
    void LogAudit(string eventName, string? category = null, string? source = null, Dictionary<string, object>? data = null, string? correlationId = null);
}

public class LoggerService : ILoggerService
{
    private readonly IEnumerable<ITraceLogHandler> _traceLogHandlers;
    private readonly IEnumerable<IAuditLogHandler> _auditLogHandlers;

    public LoggerService(
        IEnumerable<ITraceLogHandler> traceLogHandlers,
        IEnumerable<IAuditLogHandler> auditLogHandlers)
    {
        ArgumentNullException.ThrowIfNull(traceLogHandlers);
        ArgumentNullException.ThrowIfNull(auditLogHandlers);
        _traceLogHandlers = traceLogHandlers;
        _auditLogHandlers = auditLogHandlers;
    }

    public void LogTrace(
         LogLevel level,
         string message,
         Exception? exception = null,
         string? category = null,
         string? source = null,
         Dictionary<string, object>? context = null,
         string? correlationId = null)
    {
        LogEntry<TracePayload> entry = LogEntryFactory.CreateWithTracePayload(
            level: level,
            message: message,
            exception: exception,
            category: category,
            source: source,
            context: context,
            correlationId: correlationId);

        foreach (ITraceLogHandler handler in _traceLogHandlers)
        {
            handler.Handle(entry);
        }
    }

    public void LogAudit(
        string eventName,
        string? category = null,
        string? source = null,
        Dictionary<string, object>? data = null,
        string? correlationId = null)
    {
        LogEntry<AuditPayload> entry = LogEntryFactory.CreateWithAuditPayload(
            eventName: eventName,
            category: category,
            source: source,
            data: data,
            correlationId: correlationId);

        foreach (IAuditLogHandler handler in _auditLogHandlers)
        {
            handler.Handle(entry);
        }
    }
}
