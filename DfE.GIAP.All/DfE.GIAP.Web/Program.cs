using System;
using System.Security.Cryptography;
using DfE.GIAP.Common.Helpers.HostEnvironment;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.Extensions.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Contents;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .ConfigureSettings();

var configuration = builder.Configuration;

// Services configuration
builder.Services
    .AddFeaturesSharedDependencies()
    .AddNewsArticleDependencies()
    .AddContentDependencies()
    .AddRoutingConfiguration()
    .AddAppConfigurationSettings(configuration)
    .AddHstsConfiguration()
    .AddFormOptionsConfiguration()
    .AddApplicationInsightsTelemetry()
    .AddAllServices()
    .AddClarity(configuration)
    .AddDsiAuthentication(configuration)
    .AddAuthConfiguration()
    .AddCookieAndSessionConfiguration()
    .AddAzureAppConfiguration()
    .AddFeatureFlagConfiguration(configuration);

var app = builder.Build();

// Error handling
if (app.Environment.IsLocal())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Exception");
    app.UseAzureAppConfiguration();
}

app.UseHsts();

// Middleware pipeline
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseConsentCheck();
app.UseSecurityHeadersMiddleware(configuration);

ClaritySettings claritySettings = configuration
    .GetSection("Clarity")
    .Get<ClaritySettings>();

app.Use(async (context, next) =>
{
    if (claritySettings != null && !string.IsNullOrEmpty(claritySettings.ProjectId))
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items["CSPNonce"] = nonce;
        context.Response.Headers["Content-Security-Policy"] =
            $"default-src 'self'; script-src 'self' 'nonce-{nonce}';";

        context.Response.Headers["Content-Security-Policy"] =
            $"script-src 'self' https://www.clarity.ms 'nonce-{nonce}'; object-src 'none';";

    }

    await next();
});

// Endpoint configuration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
