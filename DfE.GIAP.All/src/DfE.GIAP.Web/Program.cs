using System.Security.Cryptography;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Common.Application.Models;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Response;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.FilterRegistration;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.Handlers;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;
using DfE.GIAP.Web.Extensions.Startup;
using DfE.GIAP.Web.Helpers.HostEnvironment;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Search;
using static DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers.LearnerSearchResponseToViewModelMapper;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .ConfigureSettings();

ConfigurationManager configuration = builder.Configuration;

// Services configuration
builder.Services
    .AddFeaturesSharedDependencies()
    .AddNewsArticleDependencies()
    .AddSearchDependencies(configuration)
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

builder.Services.AddSingleton<IMapper<
    FurtherEducationLearner, Learner>,
    FurtherEducationLearnerToViewModelMapper>();
builder.Services.AddSingleton<IMapper<
    LearnerSearchMappingContext, LearnerTextSearchViewModel>,
    LearnerSearchResponseToViewModelMapper>();
builder.Services.AddSingleton<IMapper<
    FilterData, FilterRequest>, FilterRequestMapper>();
builder.Services.AddSingleton<IMapper<
    Dictionary<string, string[]>,
    IList<FilterRequest>>, FiltersRequestMapper>();
builder.Services.AddSingleton<IMapper<
    FurtherEducationLearner, Learner>, FurtherEducationLearnerToViewModelMapper>();
builder.Services.AddSingleton<IMapper<
    SearchFacet, FilterData>, FilterResponseMapper>();
builder.Services.AddSingleton<IMapper<
    SearchFacets, List<FilterData>>, FiltersResponseMapper>();
builder.Services.AddSingleton<
    IFilterHandlerRegistry, FilterHandlerRegistry>();
builder.Services.AddSingleton<IFilterHandler>(new NameFilterHandler("SurnameLC"));
builder.Services.AddSingleton<IFilterHandler>(new NameFilterHandler("ForenameLC"));
builder.Services.AddSingleton<IFilterHandler>(new DobFilterHandler());
builder.Services.AddSingleton<IFiltersRequestFactory, FiltersRequestFactory>();
builder.Services.AddSingleton(new GenderFilterHandler("Gender"));
builder.Services.AddSingleton<IFilterHandlerRegistry>(_ =>
{
    Dictionary<string, IFilterHandler> handlers = new()
    {
        { "SurnameLC", new NameFilterHandler("SurnameLC") },
        { "ForenameLC", new NameFilterHandler("ForenameLC") },
        { "DOB", new DobFilterHandler() },
        { "Gender", new GenderFilterHandler("Gender") }
    };

    return new FilterHandlerRegistry(handlers);
});

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
