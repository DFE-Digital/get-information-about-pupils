using System;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Service.ApplicationInsightsTelemetry;

public class TelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor ??
            throw new ArgumentNullException(nameof(httpContextAccessor));
    }
    public void Initialize(ITelemetry telemetry)
    {
        if (_httpContextAccessor.HttpContext == null) return;

        string sessionId = _httpContextAccessor.HttpContext.Request.Headers.FirstOrDefault(x => x.Key == "api-client-session-id").Value.ToString();
        telemetry.Context.Session.Id = sessionId;

        string clientId = _httpContextAccessor.HttpContext.Request.Headers.FirstOrDefault(x => x.Key == "api-client-app-id").Value.ToString();
        telemetry.Context.User.Id = clientId;
    }
}
