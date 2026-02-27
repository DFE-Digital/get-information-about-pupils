using System.Security.Claims;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Configuration;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Web.Config;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Logging;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Helpers.TextSanitiser;
using DfE.GIAP.Web.Providers.Cookie;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.Services.ApiProcessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using SessionOptions = DfE.GIAP.Web.Config.SessionOptions;

namespace DfE.GIAP.Web.Extensions.Startup;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureAppSettings>(configuration)
            .Configure<MicrosoftClarityOptions>(configuration.GetSection(MicrosoftClarityOptions.SectionName))
            .Configure<GoogleTagManagerOptions>(configuration.GetSection(GoogleTagManagerOptions.SectionName))
            .Configure<LoggingOptions>(configuration.GetSection(LoggingOptions.SectionName)) // TODO: Move
            .Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));

        return services;
    }

    internal static IServiceCollection AddAllServices(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddHttpClient<IApiService, ApiService>();
        services.AddScoped<ISelectionManager, NotSelectedManager>();
        services.AddScoped<ITextSearchSelectionManager, TextSearchSelectionManager>();
        services.AddSingleton<ITextSanitiserHandler, HtmlTextSanitiser>();
        services.AddScoped<IApplicationLogEntryFactory<TracePayloadOptions, TracePayload>, TraceLogFactory>();
        services.AddScoped<IBusinessEventFactory, BusinessEventFactory>();

        return services;
    }

    internal static IServiceCollection AddWebProviders(this IServiceCollection services)
    {
        services.AddScoped<ISessionProvider, SessionProvider>();
        services.AddScoped<ICookieProvider, CookieProvider>();

        return services;
    }

    internal static IServiceCollection AddHstsConfiguration(this IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        return services;
    }

    internal static IServiceCollection AddFormOptionsConfiguration(this IServiceCollection services)
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

    internal static IServiceCollection AddAuthConfiguration(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policy.RequiresManageContentAccess, policy =>
                policy.RequireRole(AuthRoles.Admin));

        services.AddControllersWithViews(config =>
        {
            AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                             .RequireAuthenticatedUser()
                             .RequireClaim(ClaimTypes.Role)
                             .Build();
            config.Filters.Add(new AuthorizeFilter(policy));
        })
            .AddSessionStateTempDataProvider()
            .AddControllersAsServices();

        return services;
    }

    internal static IServiceCollection AddCookieAndSessionConfiguration(this IServiceCollection services)
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

    internal static IServiceCollection AddRoutingConfiguration(this IServiceCollection services)
    {
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
        });

        return services;
    }
}
