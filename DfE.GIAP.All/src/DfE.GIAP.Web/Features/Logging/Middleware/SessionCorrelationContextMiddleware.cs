using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Logging.Middleware;

public class SessionCorrelationContextMiddleware
{
    private readonly RequestDelegate _next;
    public SessionCorrelationContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISessionProvider sessionProvider)
    {
        string correlationId = sessionProvider
            .GetSessionValueOrDefault<string>(SessionKeys.CorrelationId);

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            sessionProvider.SetSessionValue<string>(SessionKeys.CorrelationId, correlationId);
        }

        // TODO: Remove once tested
        Console.WriteLine(
            $"[CORRELATIONID]: {correlationId} | " +
            $"[USERID]: {context.User.GetUserId()} | " +
            $"[SESSIONID]: {context.User.GetSessionId()}");

        await _next(context);
    }
}
