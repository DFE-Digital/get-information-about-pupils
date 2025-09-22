using System.Security.Claims;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

public class LogEntryFactory : ILogEntryFactory
{
    private readonly ISessionProvider _sessionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogEntryFactory(ISessionProvider sessionProvider, IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _sessionProvider = sessionProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public LogEntry<TracePayload> CreateTraceLogEntry(
        LogLevel level,
        string message,
        Exception exception = null,
        string category = null,
        string source = null,
        Dictionary<string, object> context = null)
    {
        string? correlationId = _sessionProvider.GetSessionValue(SessionKeys.CorrelationId);

        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        string userId = user?.GetUserId();
        string sessionId = user?.GetSessionId();

        TracePayload payload = new(
            Message: message,
            CorrelationId: correlationId,
            UserID: userId,
            SessionId: sessionId,
            Level: level,
            Exception: exception,
            Category: category,
            Source: source,
            Context: context);

        return new LogEntry<TracePayload>
        {
            Timestamp = DateTime.UtcNow,
            Payload = payload
        };
    }
}
