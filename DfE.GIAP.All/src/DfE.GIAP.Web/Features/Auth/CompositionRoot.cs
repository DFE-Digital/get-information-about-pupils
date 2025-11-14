using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using DfE.GIAP.Web.Features.Auth.Application;
using DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;
using DfE.GIAP.Web.Features.Auth.Infrastructure;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Web.Features.Auth;

public static class CompositionRoot
{
    public static IServiceCollection AddAuthDependencies(this IServiceCollection services, IConfiguration config)
    {
        // Bind strongly typed settings
        services.Configure<DsiOptions>(config.GetSection(DsiOptions.SectionName));

        // Core application services
        services.AddScoped<IClaimsEnricher, DfeClaimsEnricher>();
        services.AddSingleton<ISigningCredentialsProvider, SymmetricSigningCredentialsProvider>();

        services.AddScoped<IPostTokenValidatedHandler, ClaimsEnrichmentHandler>();
        services.AddScoped<IPostTokenValidatedHandler, CreateUserIfNotExistHandler>();
        services.AddScoped<IPostTokenValidatedHandler, SetUnreadNewsStatusHandler>();
        services.AddScoped<IPostTokenValidatedHandler, UpdateUserLastLoggedInHandler>();
        services.AddScoped<IPostTokenValidatedHandler, TempLoggingHandler>();

        services.AddScoped<PostTokenHandlerBuilder>();
        services.AddScoped<OidcEventsHandler>(sp =>
        {
            PostTokenHandlerBuilder builder = sp.GetRequiredService<PostTokenHandlerBuilder>();
            IReadOnlyList<IPostTokenValidatedHandler> orderedHandlers = builder
                .StartWith<ClaimsEnrichmentHandler>()
                .Then<CreateUserIfNotExistHandler>()
                .Then<SetUnreadNewsStatusHandler>()
                .Then<UpdateUserLastLoggedInHandler>()
                .Then<TempLoggingHandler>()
                .Build();

            IOptions<DsiOptions> options = sp.GetRequiredService<IOptions<DsiOptions>>();
            return new OidcEventsHandler(orderedHandlers, options);
        });

        services.AddHttpClient<IDfeSignInApiClient, DfeHttpSignInApiClient>((sp, client) =>
        {
            DsiOptions options = sp.GetRequiredService<IOptions<DsiOptions>>().Value;
            ISigningCredentialsProvider credentialsProvider = sp.GetRequiredService<ISigningCredentialsProvider>();

            string token = new JwtSecurityTokenHandler().CreateEncodedJwt(
                new SecurityTokenDescriptor
                {
                    Issuer = options.ClientId,
                    Audience = options.Audience,
                    SigningCredentials = credentialsProvider.GetSigningCredentials()
                });

            client.BaseAddress = new Uri(options.AuthorisationUrl.TrimEnd('/'));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });

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

