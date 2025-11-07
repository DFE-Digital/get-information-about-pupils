using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Infrastructure;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Auth;

public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration config)
{
    var appSettings = config.Get<AzureAppSettings>();
    services.AddSingleton(appSettings);

    services.AddScoped<IClaimsEnricher, DfeClaimsEnricher>();
    services.AddScoped<IUserContextFactory, UserContextFactory>();
    services.AddScoped<OidcEventsHandler>();

    // Register use cases (already implemented in Core.Users.Application)
    services.AddScoped<IUseCaseRequestOnly<CreateUserIfNotExistsRequest>, CreateUserIfNotExistsUseCase>();
    services.AddScoped<IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse>, GetUnreadUserNewsUseCase>();
    services.AddScoped<IUseCaseRequestOnly<UpdateLastLoggedInRequest>, UpdateLastLoggedInUseCase>();

    services.AddAuthentication(options =>
    {
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(o =>
    {
        o.ExpireTimeSpan = TimeSpan.FromMinutes(appSettings.SessionTimeout);
        o.SlidingExpiration = true;
    })
    .AddOpenIdConnect(o =>
    {
        o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        o.MetadataAddress = appSettings.DsiMetadataAddress;
        o.ClientId = appSettings.DsiClientId;
        o.ClientSecret = appSettings.DsiClientSecret;
        o.ResponseType = OpenIdConnectResponseType.Code;
        o.Scope.Add("openid");
        o.Scope.Add("email");
        o.Scope.Add("profile");
        o.CallbackPath = "/signin-oidc";
        o.SignedOutCallbackPath = "/signout-callback-oidc";

        // Resolve handler from DI
        o.Events.OnMessageReceived = ctx =>
            ctx.HttpContext.RequestServices.GetRequiredService<OidcEventsHandler>().OnMessageReceived(ctx);
        o.Events.OnRemoteFailure = ctx =>
            ctx.HttpContext.RequestServices.GetRequiredService<OidcEventsHandler>().OnRemoteFailure(ctx);
        o.Events.OnTokenValidated = ctx =>
            ctx.HttpContext.RequestServices.GetRequiredService<OidcEventsHandler>().OnTokenValidated(ctx);
    });

    return services;
}

