using System.Security.Claims;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Logging;

public class TracePayloadEnricher : ILogPayloadEnricher<TracePayload, TracePayloadOptions>
{
    private readonly ISessionProvider _sessionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TracePayloadEnricher(ISessionProvider sessionProvider, IHttpContextAccessor httpContextAccessor)
    {
        _sessionProvider = sessionProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public TracePayload Enrich(TracePayloadOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        string correlationId = _sessionProvider.GetSessionValueOrDefault<string>(SessionKeys.CorrelationId);
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        string userId = user?.GetUserId();
        string sessionId = user?.GetSessionId();

        return new TracePayload(
            Message: options.Message,
            CorrelationId: correlationId,
            UserID: userId,
            SessionId: sessionId,
            Level: options.Level,
            Exception: options.Exception,
            Category: options.Category,
            Source: options.Source,
            Context: options.Context);
    }
}
