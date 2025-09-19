using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Middleware;

public class SessionCorrelationContextMiddleware
{
    private readonly RequestDelegate _next;
    public SessionCorrelationContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISessionProvider sessionProvider,
        ICorrelationContextAccessor accessor)
    {
        string correlationId = sessionProvider
            .GetSessionValueOrDefault<string>("CorrelationId");

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            sessionProvider.SetSessionValue<string>("CorrelationId", correlationId);
        }

        accessor.CorrelationId = correlationId;
        accessor.UserId = context.User?.GetUserId();
        accessor.SessionId = context.User?.GetSessionId();

        Console.WriteLine(
            $"[CORRELATIONID]: {accessor.CorrelationId} | " +
            $"[USERID]: {accessor.UserId} | " +
            $"[SESSIONID]: {accessor.SessionId}");

        await _next(context);
    }
}
