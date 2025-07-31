using System.Security.Cryptography;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Web.Helpers.HostEnvironment;
using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Contents;
using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Web.Extensions.Startup;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Core.Users;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .ConfigureSettings();

ConfigurationManager configuration = builder.Configuration;

// Services configuration
builder.Services
    .AddFeaturesSharedDependencies()
    .AddNewsArticleDependencies()
    .AddUserDependencies()
    .AddContentDependencies()
    .AddRoutingConfiguration()
    .AddAppConfigurationSettings(configuration)
    .AddHstsConfiguration()
    .AddFormOptionsConfiguration()
    .AddApplicationInsightsTelemetry()
    .AddAllServices()
    .AddWebProviders()
    .AddSettings<ClaritySettings>(configuration, "Clarity")
    .AddSettings<GoogleTagManager>(configuration, "GoogleTagManager")
    .AddDsiAuthentication(configuration)
    .AddAuthConfiguration()
    .AddCookieAndSessionConfiguration()
    .AddAzureAppConfiguration()
    .AddFeatureFlagConfiguration(configuration);

WebApplication app = builder.Build();

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
        string nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items["CSPNonce"] = nonce;

        context.Response.Headers.ContentSecurityPolicy = $"script-src 'self' https://www.clarity.ms https://www.googletagmanager.com 'nonce-{nonce}'; object-src 'none';";
    }

    await next();
});

// Endpoint configuration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
