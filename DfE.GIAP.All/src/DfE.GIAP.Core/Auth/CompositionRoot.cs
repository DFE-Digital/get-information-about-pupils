using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Infrastructure;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using DfE.GIAP.Core.NewsArticles;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace DfE.GIAP.Core.Auth;

public static class CompositionRoot
{
    public static IServiceCollection AddAuthDependencies(this IServiceCollection services, IConfiguration config)
    {
        // Bind strongly typed settings
        services.Configure<DsiOptions>(config.GetSection(DsiOptions.SectionName));

        // Core application services
        services.AddScoped<IClaimsEnricher, DfeClaimsEnricher>();
        services.AddScoped<IUserContextFactory, UserContextFactory>();
        services.AddScoped<ISigningCredentialsProvider, SymmetricSigningCredentialsProvider>();

        services.AddScoped<IPostTokenValidatedHandler, ClaimsEnrichmentHandler>();
        services.AddScoped<IPostTokenValidatedHandler, UserPostLoginHandler>();
        services.AddScoped<OidcEventsHandler>();

        // Register use cases
        services.AddNewsArticleDependencies(); // TODO: Remove when Auth no longer depends on NewsArticles

        // Register typed HttpClient for the API client
        services.AddHttpClient<IDfeSignInApiClient, DfeHttpSignInApiClient>();

        // Configure OIDC authentication if settings are present
        DsiOptions? dsiOptions = config.GetSection(DsiOptions.SectionName).Get<DsiOptions>();
        ArgumentNullException.ThrowIfNull(dsiOptions);

        if (!string.IsNullOrEmpty(dsiOptions?.ClientId))
        {
            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                o.ExpireTimeSpan = TimeSpan.FromMinutes(dsiOptions.SessionTimeoutMinutes);
                o.SlidingExpiration = true;
                o.LogoutPath = "/auth/logout";

                o.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = 403;
                    ctx.Response.Redirect("/user-with-no-role");
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(o =>
            {
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.MetadataAddress = dsiOptions.MetadataAddress;
                o.ClientId = dsiOptions.ClientId;
                o.ClientSecret = dsiOptions.ClientSecret;
                o.ResponseType = OpenIdConnectResponseType.Code;
                o.RequireHttpsMetadata = true;
                o.GetClaimsFromUserInfoEndpoint = true;
                o.SaveTokens = true;
                o.CallbackPath = dsiOptions.CallbackPath;
                o.SignedOutCallbackPath = dsiOptions.SignedOutCallbackPath;
                o.DisableTelemetry = true;
                o.TokenHandler = new JsonWebTokenHandler
                {
                    InboundClaimTypeMap = new Dictionary<string, string>(),
                    TokenLifetimeInMinutes = 90,
                    SetDefaultTimesOnTokenCreation = true
                };
                o.ProtocolValidator = new OpenIdConnectProtocolValidator
                {
                    RequireSub = true,
                    RequireStateValidation = false,
                    NonceLifetime = TimeSpan.FromMinutes(60)
                };

                o.Scope.Clear();
                o.Scope.Add("openid");
                o.Scope.Add("email");
                o.Scope.Add("profile"); // TODO: Remove as not pulled back?
                o.Scope.Add("organisationid");

                // Resolve handler from DI
                o.Events.OnMessageReceived = ctx =>
                    ctx.HttpContext.RequestServices.GetRequiredService<OidcEventsHandler>().OnMessageReceived(ctx);
                o.Events.OnRemoteFailure = ctx =>
                    ctx.HttpContext.RequestServices.GetRequiredService<OidcEventsHandler>().OnRemoteFailure(ctx);
                o.Events.OnTokenValidated = ctx =>
                    ctx.HttpContext.RequestServices.GetRequiredService<OidcEventsHandler>().OnTokenValidated(ctx);
            });
        }

        return services;
    }
}

