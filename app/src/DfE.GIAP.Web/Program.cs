using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Downloads;
using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Core.PreparedDownloads;
using DfE.GIAP.Core.Users;
using DfE.GIAP.Web.Extensions.Startup;
using DfE.GIAP.Web.Features.Auth;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.Logging.Middleware;
using DfE.GIAP.Web.Features.Logging.Sentry;
using DfE.GIAP.Web.Features.MyPupils;
using DfE.GIAP.Web.Features.Search;
using DfE.GIAP.Web.Helpers.HostEnvironment;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Shared.Session;
using DfE.GIAP.Web.Shared.TempData;
using GovUk.Frontend.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .ConfigureSettings();

ConfigurationManager configuration = builder.Configuration;

if (builder.Environment.IsProduction())
{
    string sentryDsn = configuration["Sentry:Dsn"];
    if (string.IsNullOrWhiteSpace(sentryDsn))
    {
        throw new InvalidOperationException("Sentry:Dsn must be configured in Production.");
    }

    builder.WebHost.UseSentry(dsn: sentryDsn);
    builder.Services.AddSingleton<Sentry.Extensibility.ISentryEventProcessor, RemoveRequestUrlEventProcessor>();
}

// Services configuration
builder.Services
    .AddAppSettings(configuration)
    .AddFeaturesSharedDependencies()
    .AddAspNetCoreSessionServices()
    .AddTempDataProvider()
    .AddJsonSerializer()
    .AddUserDependencies()
    .AddNewsArticleDependencies()
    .AddPrePreparedDownloadsDependencies()
    .AddDownloadDependencies(configuration)
    .AddScoped<IDownloadPupilPremiumPupilDataService, DownloadPupilPremiumPupilDataService>()
    .AddAuthDependencies(configuration)
    .AddMyPupils(configuration)
    .AddSearch(configuration);

builder.Services
    .AddRoutingConfiguration()
    .AddHstsConfiguration()
    .AddFormOptionsConfiguration()
    .AddApplicationInsightsTelemetry() // TODO: This would move to infrastructure, handle IHostingEnvironment within tests
    .AddAllServices()
    .AddWebProviders()
    .AddAuthConfiguration()
    .AddCookieAndSessionConfiguration();

builder.Services.AddHealthChecks();

builder.Services.AddGovUkFrontend();

if (builder.Environment.IsLocal())
{
    // Watches Styles/*.scss and recompiles wwwroot/css/app.css while the app is running.
    builder.Services.AddSassCompiler();
}

WebApplication app = builder.Build();

// Error handling
if (app.Environment.IsLocal())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Exception");
}

app.UseHsts();

// Middleware pipeline
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseHttpsRedirection();
app.UseGovUkFrontend();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseConsentCheck();
app.UseSecurityHeadersMiddleware(configuration);
app.UseMiddleware<SessionCorrelationIdMiddleware>();

// Endpoint configuration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/status").WithMetadata(new AllowWithoutConsentAttribute());

app.Run();
