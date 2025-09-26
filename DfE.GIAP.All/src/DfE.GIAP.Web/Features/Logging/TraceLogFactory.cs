using System.Security.Claims;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Logging;

public class TraceLogFactory : ILogEntryFactory<TracePayloadOptions, TracePayload>
{
    private readonly ISessionProvider _sessionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TraceLogFactory(
        ISessionProvider sessionProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _sessionProvider = sessionProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public Log<TracePayload> Create(TracePayloadOptions options)
    {
        string correlationId = _sessionProvider.GetSessionValueOrDefault<string>(SessionKeys.CorrelationId);
        ClaimsPrincipal user = _httpContextAccessor.HttpContext?.User;

        TracePayload payload = new(
            Message: options.Message ?? string.Empty,
            CorrelationId: correlationId,
            UserID: user?.GetUserId(),
            SessionId: user?.GetSessionId(),
            Level: options.Level,
            Exception: options.Exception,
            Category: options.Category,
            Source: options.Source,
            Context: options.Context);

        return new Log<TracePayload>
        {
            Timestamp = DateTime.UtcNow,
            Payload = payload
        };
    }
}

