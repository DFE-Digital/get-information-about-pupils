using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Http.Features;

namespace DfE.GIAP.Web.Middleware;

public class ConsentRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISessionProvider _sessionProvider;

    public ConsentRedirectMiddleware(RequestDelegate next, ISessionProvider sessionProvider)
    {
        _next = next;
        _sessionProvider = sessionProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        HttpRequest request = context.Request;
        HttpResponse response = context.Response;

        if (context.User.Identity.IsAuthenticated)
        {

            Endpoint endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            AllowWithoutConsentAttribute attribute = endpoint?.Metadata.GetMetadata<AllowWithoutConsentAttribute>();
            if (attribute is null)
            {
                // TODO: Why are we using "yes" ?
                // Check if consent key is missing or its value is not "yes"
                string consentValue = _sessionProvider.GetSessionValue(SessionKeys.ConsentGiven);
                if (!string.Equals(consentValue, SessionKeys.ConsentGiven, StringComparison.OrdinalIgnoreCase))
                {
                    response.Redirect(Routes.Application.Consent);
                    return;
                }
            }
        }

        await _next(context);
    }
}

[ExcludeFromCodeCoverage]
public static class ConsentRequestExtensions
{
    public static IApplicationBuilder UseConsentCheck(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ConsentRedirectMiddleware>();
    }
}
