using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.SearchRules;
using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Downloads;
using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Core.PreparedDownloads;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Users;
using DfE.GIAP.Web.Features.MyPupils;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Controllers.LearnerNumber.Mappers;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.FilterRegistration;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.Handlers;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;
using DfE.GIAP.Web.Extensions.Startup;
using DfE.GIAP.Web.Features.Auth;
using DfE.GIAP.Web.Features.Logging.Middleware;
using DfE.GIAP.Web.Helpers.HostEnvironment;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels.Search;
using static DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers.LearnerTextSearchResponseToViewModelMapper;
using Learner = DfE.GIAP.Core.Search.Application.Models.Learner.Learner;
using DfE.GIAP.Web.Shared.Session;
using DfE.GIAP.Web.Shared.TempData;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Web.Features.Downloads.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .ConfigureSettings();

ConfigurationManager configuration = builder.Configuration;

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
    .AddDownloadDependencies()
    .AddScoped<IDownloadPupilPremiumPupilDataService, DownloadPupilPremiumPupilDataService>()
    .AddAuthDependencies(configuration)
    .AddMyPupils();

builder.Services
    .AddSearchDependencies(configuration)
    .AddRoutingConfiguration()
    .AddHstsConfiguration()
    .AddFormOptionsConfiguration()
    .AddApplicationInsightsTelemetry() // TODO: This would move to infrastructure, handle IHostingEnvironment within tests
    .AddAllServices()
    .AddWebProviders()
    .AddAuthConfiguration()
    .AddCookieAndSessionConfiguration();

builder.Services.AddSingleton<IMapper<
    LearnerTextSearchMappingContext, LearnerTextSearchViewModel>,
    LearnerTextSearchResponseToViewModelMapper>();

builder.Services.AddSingleton<IMapper<
    LearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>,
    LearnerNumericSearchResponseToViewModelMapper>();

builder.Services.AddSingleton<IMapper<
    FilterData, FilterRequest>, FilterRequestMapper>();
builder.Services.AddSingleton<IMapper<
    Dictionary<string, string[]>,
    IList<FilterRequest>>, FiltersRequestMapper>();
builder.Services.AddSingleton<IMapper<
    Learner, DfE.GIAP.Domain.Search.Learner.Learner>, LearnerToViewModelMapper>();
builder.Services.AddSingleton<IMapper<
    SearchFacet, FilterData>, FilterResponseMapper>();
builder.Services.AddSingleton<IMapper<
    SearchFacets, List<FilterData>>, FiltersResponseMapper>();
builder.Services.AddSingleton<IMapper<
    (string, string), SortOrder>, SortOrderMapper>();

builder.Services.AddSingleton<ISearchRule, PartialWordMatchRule>();

builder.Services.AddSingleton<IFiltersRequestFactory, FiltersRequestFactory>();
builder.Services.AddSingleton<IFilterHandlerRegistry>(_ =>
{
    Dictionary<string, IFilterHandler> handlers = new()
    {
        { "SurnameLC", new NameFilterHandler("SurnameLC") },
        { "ForenameLC", new NameFilterHandler("ForenameLC") },
        { "DOB", new DobFilterHandler() },
        { "Sex", new GenderFilterHandler("Sex") }
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
app.UseMiddleware<SessionCorrelationIdMiddleware>();

// Endpoint configuration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
