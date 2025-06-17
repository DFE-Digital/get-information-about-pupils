using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Helpers.Consent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace DfE.GIAP.Web.Middleware;

public class ConsentRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public ConsentRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (context.User.Identity.IsAuthenticated)
        {

            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var attribute = endpoint?.Metadata.GetMetadata<AllowWithoutConsentAttribute>();
            if (attribute == null)
            {
                if (!ConsentHelper.HasGivenConsent(context))
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
