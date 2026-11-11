using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Logging.Middleware;

public class SessionCorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    public SessionCorrelationIdMiddleware(RequestDelegate next)
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

        await _next(context);
    }
}
