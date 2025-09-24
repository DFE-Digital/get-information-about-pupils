using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Core.PreparedDownloads;
using DfE.GIAP.Core.Users;
using DfE.GIAP.Web.Extensions.Startup;
using DfE.GIAP.Web.Features.Logging.Middleware;
using DfE.GIAP.Web.Helpers.HostEnvironment;
using DfE.GIAP.Web.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .ConfigureSettings();

ConfigurationManager configuration = builder.Configuration;

// Services configuration
builder.Services
    .AddAppSettings(configuration)
    .AddFeaturesSharedDependencies(configuration)
    .AddUserDependencies()
    .AddNewsArticleDependencies()
    .AddPrePreparedDownloadsDependencies();

builder.Services
    .AddRoutingConfiguration()
    .AddHstsConfiguration()
    .AddFormOptionsConfiguration()
    .AddApplicationInsightsTelemetry() // TODO: This would move to infrastructure
    .AddAllServices()
    .AddWebProviders()
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
app.UseMiddleware<SessionCorrelationContextMiddleware>();

// Endpoint configuration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
