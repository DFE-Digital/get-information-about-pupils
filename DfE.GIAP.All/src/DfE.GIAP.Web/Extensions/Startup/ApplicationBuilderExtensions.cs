using System.Security.Cryptography;
using DfE.GIAP.Common.AppSettings;

namespace DfE.GIAP.Web.Extensions.Startup;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSecurityHeadersMiddleware(this IApplicationBuilder app, IConfiguration configuration)
    {
        SecurityHeadersSettings securityHeaders = configuration
            .GetSection(SecurityHeadersSettings.SectionName)
            .Get<SecurityHeadersSettings>();

        app.Use(async (context, next) =>
        {
            IHeaderDictionary headers = context.Response.Headers;

            // Generate nonce and store in context
            string nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            context.Items["CSPNonce"] = nonce;

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
                        string value = header.Key.Equals("Content-Security-Policy", StringComparison.OrdinalIgnoreCase)
                            ? header.Value.Replace("{nonce}", nonce)
                            : header.Value;

                        headers[header.Key] = value;
                    }
                }
            }

            await next.Invoke();
        });

        return app;
    }
}
