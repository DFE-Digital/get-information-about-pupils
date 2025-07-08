using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Web.Extensions.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Middleware
{
    public class SecurityHeadersMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_adds_specified_headers()
        {
            // Arrange
            Dictionary<string, string> headersToAdd = new()
            {
                { "Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload" },
                { "X-Frame-Options", "DENY" }
            };

            HttpContext context = CreateContext();
            RequestDelegate middleware = CreateMiddleware(new SecurityHeadersSettings { Add = headersToAdd });

            // Act
            await middleware.Invoke(context);

            // Assert
            foreach (KeyValuePair<string, string> header in headersToAdd)
            {
                Assert.True(context.Response.Headers.ContainsKey(header.Key), $"Header {header.Key} is missing.");
                Assert.Equal(header.Value, context.Response.Headers[header.Key]);
            }
        }

        [Fact]
        public async Task InvokeAsync_removes_specified_headers()
        {
            // Arrange
            List<string> headersToRemove = new() { "Server", "X-Powered-By" };
            HttpContext context = CreateContext(headersToRemove);

            RequestDelegate middleware = CreateMiddleware(new SecurityHeadersSettings { Remove = headersToRemove });

            // Act
            await middleware.Invoke(context);

            // Assert
            foreach (string header in headersToRemove)
            {
                Assert.False(context.Response.Headers.ContainsKey(header), $"Header {header} was not removed.");
            }
        }

        [Fact]
        public async Task InvokeAsync_does_not_alter_headers_when_no_settings_provided()
        {
            // Arrange
            HttpContext context = CreateContext();
            RequestDelegate middleware = CreateMiddleware(null);

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.Empty(context.Response.Headers);
        }

        private static HttpContext CreateContext(List<string> preExistingHeaders = null)
        {
            DefaultHttpContext context = new DefaultHttpContext();

            // Pre-populate headers if provided
            if (preExistingHeaders is not null)
            {
                foreach (string header in preExistingHeaders)
                {
                    context.Response.Headers[header] = "dummy-value";
                }
            }

            return context;
        }

        private static RequestDelegate CreateMiddleware(SecurityHeadersSettings settings)
        {
            IConfiguration config = CreateMockConfiguration(settings);
            ApplicationBuilder builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            builder.UseSecurityHeadersMiddleware(config);
            return builder.Build();
        }

        private static IConfiguration CreateMockConfiguration(SecurityHeadersSettings settings)
        {
            Dictionary<string, string> inMemorySettings = new();

            if (settings is null) return BuildConfiguration(inMemorySettings);

            if (settings.Add is not null)
            {
                foreach (KeyValuePair<string, string> header in settings.Add)
                {
                    inMemorySettings[$"{SecurityHeadersSettings.SectionName}:Add:{header.Key}"] = header.Value;
                }
            }

            if (settings.Remove is not null)
            {
                for (int i = 0; i < settings.Remove.Count; i++)
                {
                    inMemorySettings[$"{SecurityHeadersSettings.SectionName}:Remove:{i}"] = settings.Remove[i];
                }
            }

            return BuildConfiguration(inMemorySettings);
        }

        private static IConfiguration BuildConfiguration(Dictionary<string, string> inMemorySettings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }
    }
}
