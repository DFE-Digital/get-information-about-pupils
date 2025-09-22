using System.Security.Claims;
using DfE.GIAP.Core.Logging.Application;
using DfE.GIAP.Core.Logging.Application.Models;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Logging;

public class TraceLogPayloadBuilder : ILogPayloadBuilder<TracePayload>
{
    private readonly ISessionProvider _sessionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TraceLogPayloadBuilder(ISessionProvider sessionProvider, IHttpContextAccessor httpContextAccessor)
    {
        _sessionProvider = sessionProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public TracePayload BuildPayload(ILogPayloadOptions options)
    {
        TracePayloadOptions traceOptions = options as TracePayloadOptions
            ?? throw new ArgumentException("Invalid options type for TracePayload");

        string correlationId = _sessionProvider.GetSessionValueOrDefault<string>(SessionKeys.CorrelationId);
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        string userId = user?.GetUserId();
        string sessionId = user?.GetSessionId();

        return new TracePayload(
            Message: traceOptions.Message,
            CorrelationId: correlationId,
            UserID: userId,
            SessionId: sessionId,
            Level: traceOptions.Level,
            Exception: traceOptions.Exception,
            Category: traceOptions.Category,
            Source: traceOptions.Source,
            Context: traceOptions.Context);
    }
}
