using System;
using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.DsiConfiguration;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using DfE.GIAP.Service.BlobStorage;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Service.Download.SecurityReport;
using DfE.GIAP.Service.DsiApiClient;
using DfE.GIAP.Service.ManageDocument;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.News;
using DfE.GIAP.Service.PreparedDownloads;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Service.Security;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace DfE.GIAP.Web.Extensions.Startup
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureAppSettings>(configuration);

            return services;
        }

        public static IServiceCollection AddFeatureFlagConfiguration(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddFeatureManagement(configuration);

            AzureAppSettings appSettings = configuration.Get<AzureAppSettings>();
            string connectionUrl = appSettings.FeatureFlagAppConfigUrl;
            if (!string.IsNullOrWhiteSpace(connectionUrl))
            {
                configuration.AddAzureAppConfiguration(options =>
                    options.Connect(connectionUrl).UseFeatureFlags());
            }

            return services;
        }

        public static IServiceCollection AddAllServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpClient<IApiService, ApiService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.AddScoped<ICookieManager, CookieManager>();
            services.AddScoped<IManageDocumentsService, ManageDocumentsService>();
            services.AddScoped<ISecurityKeyProvider, SymmetricSecurityKeyProvider>();
            services.AddHttpClient<IDsiHttpClientProvider, DsiHttpClientProvider>();
            services.AddScoped<IDfeSignInApiClient, DfeSignInApiClient>();
            services.AddScoped<IDownloadService, DownloadService>();
            services.AddScoped<IUlnDownloadService, UlnDownloadService>();
            services.AddScoped<IContentService, ContentService>();
            services.AddSingleton<ISecurityService, SecurityService>();
            services.AddScoped<IPrePreparedDownloadsService, PrePreparedDownloadsService>();
            services.AddScoped<IDownloadCommonTransferFileService, DownloadCommonTransferFileService>();
            services.AddScoped<IDownloadSecurityReportByUpnUlnService, DownloadSecurityReportByUpnUlnService>();
            services.AddScoped<IDownloadSecurityReportLoginDetailsService, DownloadSecurityReportLoginDetailsService>();
            services.AddScoped<IDownloadSecurityReportDetailedSearchesService, DownloadSecurityReportDetailedSearchesService>();
            services.AddScoped<IPaginatedSearchService, PaginatedSearchService>();
            services.AddScoped<ISelectionManager, NotSelectedManager>();
            services.AddScoped<ITextSearchSelectionManager, TextSearchSelectionManager>();
            services.AddScoped<IMyPupilListService, MyPupilListService>();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
            services.AddTransient<IEventLogging, EventLogging>();
            services.AddScoped<ILatestNewsBanner, LatestNewsBanner>();
            services.AddScoped<ISessionProvider, SessionProvider>();
            return services;
        }

        public static IServiceCollection AddHstsConfiguration(this IServiceCollection services)
        {
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            return services;
        }

        public static IServiceCollection AddFormOptionsConfiguration(this IServiceCollection services)
        {
            services.Configure<FormOptions>(x =>
            {
                x.BufferBody = false;
                x.KeyLengthLimit = 2048; // 2 KiB
                x.ValueLengthLimit = 4194304; // 32 MiB
                x.ValueCountLimit = 8092;// 1024
                x.MultipartHeadersCountLimit = 32; // 16
                x.MultipartHeadersLengthLimit = 32768; // 16384
                x.MultipartBoundaryLengthLimit = 256; // 128
                x.MultipartBodyLengthLimit = 134217728; // 128 MiB
            });

            return services;
        }

        public static IServiceCollection AddAuthConfiguration(this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy(Policy.RequireAdminApproverAccess, policy =>
                    policy.RequireRole(Role.Admin, Role.Approver));

            services.AddControllersWithViews(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .RequireClaim(ClaimTypes.Role)
                                 .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddSessionStateTempDataProvider()
                .AddControllersAsServices();

            return services;
        }

        public static IServiceCollection AddCookieAndSessionConfiguration(this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
            });

            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = Antiforgery.AntiforgeryCookieName;
                options.FormFieldName = Antiforgery.AntiforgeryFieldName;
                options.HeaderName = Antiforgery.AntiforgeryHeaderName;
                options.SuppressXFrameOptionsHeader = false;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            return services;
        }

        public static IServiceCollection AddRoutingConfiguration(this IServiceCollection services)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            return services;
        }

        public static IServiceCollection AddClarity(this IServiceCollection services, IConfigurationManager configuration)
        {

            services.Configure<ClaritySettings>(configuration.GetSection("Clarity"));

            return services;
        }
    }
}
