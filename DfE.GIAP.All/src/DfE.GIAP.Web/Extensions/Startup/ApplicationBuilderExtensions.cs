using DfE.GIAP.Common.AppSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace DfE.GIAP.Web.Extensions.Startup
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSecurityHeadersMiddleware(this IApplicationBuilder app, IConfiguration configuration)
        {
            SecurityHeadersSettings securityHeaders = configuration
                .GetSection(SecurityHeadersSettings.SectionName)
                .Get<SecurityHeadersSettings>();
 
            app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;

                // Remove specified headers
                if (securityHeaders?.Remove is not null)
                {
                    foreach (string header in securityHeaders.Remove)
                    {
                        headers.Remove(header);
                    }
                }

                // Add specified headers
                if (securityHeaders?.Add is not null)
                {
                    foreach (KeyValuePair<string, string> header in securityHeaders.Add)
                    {
                        if (!string.IsNullOrWhiteSpace(header.Value))
                        {
                            headers[header.Key] = header.Value;
                        }
                    }
                }

                await next.Invoke();
            });

            return app;
        }
    }
}
