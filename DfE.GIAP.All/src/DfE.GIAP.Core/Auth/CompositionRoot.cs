using System.Net.Http.Headers;
using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Infrastructure;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Core.Auth;

public static class CompositionRoot
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration config)
    {
        // Bind strongly typed settings
        services.Configure<OidcSettings>(config.GetSection("OidcSettings"));
        services.Configure<SignInApiSettings>(config.GetSection("SignInApiSettings"));

        // Core application services
        services.AddScoped<IClaimsEnricher, DfeClaimsEnricher>();
        services.AddScoped<IUserContextFactory, UserContextFactory>();
        services.AddScoped<OidcEventsHandler>();

        // Register use cases (already implemented in Core.Users.Application)
        services.AddScoped<IUseCaseRequestOnly<CreateUserIfNotExistsRequest>, CreateUserIfNotExistsUseCase>();
        services.AddScoped<IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse>, GetUnreadUserNewsUseCase>();
        services.AddScoped<IUseCaseRequestOnly<UpdateLastLoggedInRequest>, UpdateLastLoggedInUseCase>();

        // Register DfE Sign-In API client with HttpClient + IOptions
        services.AddHttpClient<IDfeSignInApiClient, DfeSignInApiClient>((sp, client) =>
        {
            SignInApiSettings settings = sp.GetRequiredService<IOptions<SignInApiSettings>>().Value;
            var keyProvider = sp.GetRequiredService<ISecurityKeyProvider>();

            // Base address
            client.BaseAddress = new Uri(settings.AuthorisationUrl.TrimEnd('/'));

            // Accept header
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Bearer token
            var token = new JwtSecurityTokenHandler().CreateEncodedJwt(
                new SecurityTokenDescriptor
                {
                    Issuer = settings.ClientId,
                    Audience = settings.Audience,
                    SigningCredentials = new SigningCredentials(
                        keyProvider.SecurityKeyInstance,
                        keyProvider.SecurityAlgorithm)
                });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });

        // Configure OIDC authentication if settings are present
        OidcSettings oidcSettings = config.GetSection("OidcSettings").Get<OidcSettings>();
        if (!string.IsNullOrEmpty(oidcSettings?.ClientId))
        {
            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                o.ExpireTimeSpan = TimeSpan.FromMinutes(oidcSettings.SessionTimeoutMinutes);
                o.SlidingExpiration = true;
            })
            .AddOpenIdConnect(o =>
            {
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.MetadataAddress = oidcSettings.MetadataAddress;
                o.ClientId = oidcSettings.ClientId;
                o.ClientSecret = oidcSettings.ClientSecret;
                o.ResponseType = OpenIdConnectResponseType.Code;
                o.CallbackPath = oidcSettings.CallbackPath;
                o.SignedOutCallbackPath = oidcSettings.SignedOutCallbackPath;

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

