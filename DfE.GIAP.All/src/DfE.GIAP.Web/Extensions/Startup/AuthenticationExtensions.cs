using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;
using DfE.GIAP.Domain.Models.LoggingEvent;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.DsiApiClient;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Helpers.DSIUser;
using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;

namespace DfE.GIAP.Web.Extensions.Startup;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddDsiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        AzureAppSettings appSettings = configuration.Get<AzureAppSettings>();

        TimeSpan overallSessionTimeout = TimeSpan.FromMinutes(appSettings.SessionTimeout);
        LoggingEvent loggingEvent = new();

        services.AddAuthentication(options =>
        {
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddCookie(options => ConfigureCookieOptions(options, overallSessionTimeout))
            .AddOpenIdConnect(options => ConfigureOpenIdConnectOptions(options, appSettings, overallSessionTimeout));

        return services;
    }

    private static void ConfigureCookieOptions(CookieAuthenticationOptions options, TimeSpan sessionTimeout)
    {
        options.ExpireTimeSpan = sessionTimeout;
        options.SlidingExpiration = true;
        options.LogoutPath = DsiKeys.CallbackPaths.Logout;

        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = 403;
            ctx.Response.Redirect($"/{Routes.Application.UserWithNoRole}");
            return Task.CompletedTask;
        };
    }

    private static void ConfigureOpenIdConnectOptions(OpenIdConnectOptions options, AzureAppSettings appSettings, TimeSpan sessionTimeout)
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.MetadataAddress = appSettings.DsiMetadataAddress;
        options.ClientId = appSettings.DsiClientId;
        options.ClientSecret = appSettings.DsiClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.RequireHttpsMetadata = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Clear();
        options.Scope.AddRange([
            DsiKeys.Scope.OpenId,
            DsiKeys.Scope.Email,
            DsiKeys.Scope.Profile,
            DsiKeys.Scope.OrganisationId
        ]);

        options.SaveTokens = true;
        options.CallbackPath = DsiKeys.CallbackPaths.Dsi;
        options.SignedOutCallbackPath = DsiKeys.CallbackPaths.SignedOut;
        options.TokenHandler = new JsonWebTokenHandler
        {
            InboundClaimTypeMap = new Dictionary<string, string>(),
            TokenLifetimeInMinutes = 90,
            SetDefaultTimesOnTokenCreation = true
        };
        options.ProtocolValidator = new OpenIdConnectProtocolValidator
        {
            RequireSub = true,
            RequireStateValidation = false,
            NonceLifetime = TimeSpan.FromMinutes(60)
        };
        options.DisableTelemetry = true;
        options.Events = new OpenIdConnectEvents
        {
            OnMessageReceived = HandleMessageReceived,
            OnRemoteFailure = HandleRemoteFailure,
            OnTokenValidated = ctx => HandleTokenValidated(ctx, appSettings, sessionTimeout)
        };
    }

    private static Task HandleMessageReceived(MessageReceivedContext context)
    {
        bool isSpuriousAuthCbRequest =
            context.Request.Path == context.Options.CallbackPath &&
            context.Request.Method == HttpMethods.Get &&
            !context.Request.Query.ContainsKey("code");

        if (isSpuriousAuthCbRequest)
        {
            context.HandleResponse();
            context.Response.Redirect(Routes.Application.Home);
        }
        return Task.CompletedTask;
    }

    private static Task HandleRemoteFailure(RemoteFailureContext ctx)
    {
        ctx.HandleResponse();
        return Task.FromException(ctx.Failure);
    }

    private static async Task HandleTokenValidated(TokenValidatedContext ctx, AzureAppSettings appSettings, TimeSpan sessionTimeout)
    {
        string sessionId = Guid.NewGuid().ToString();
        ctx.Properties.IsPersistent = true;
        ctx.Properties.ExpiresUtc = DateTime.UtcNow.Add(sessionTimeout);

        ClaimsPrincipal principal = ctx.Principal;
        string userId = principal.FindFirst("sub")?.Value ?? string.Empty;
        string userEmail = principal.FindFirst("email")?.Value ?? string.Empty;
        string userGivenName = principal.FindFirst("given_name")?.Value ?? string.Empty;
        string userSurname = principal.FindFirst("family_name")?.Value ?? string.Empty;
        string organisationJson = principal.FindFirst("organisation")?.Value ?? "{}";
        JObject organisation = JObject.Parse(organisationJson);
        string organisationId = organisation["id"]?.ToString() ?? string.Empty;

        IDfeSignInApiClient dfeSignInApiClient = ctx.HttpContext.RequestServices.GetService<IDfeSignInApiClient>();
        IEventLogging eventLogging = ctx.HttpContext.RequestServices.GetService<IEventLogging>();
        IHostEnvironment hostEnvironment = ctx.HttpContext.RequestServices.GetService<IHostEnvironment>();
        ICommonService userApiClient = ctx.HttpContext.RequestServices.GetService<ICommonService>();

        List<Claim> claims = new()
        {
            new(CustomClaimTypes.UserId, userId),
            new(CustomClaimTypes.SessionId, sessionId),
            new(ClaimTypes.Email, userEmail)
        };

        AuthenticatedUserInfo authenticatedUserInfo = new() { UserId = userId };
        if (!organisation.HasValues)
        {
            ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "DfE-SignIn"));
            ctx.HttpContext.Response.Redirect(Routes.Application.UserWithNoRole);
            return;
        }

        // HTTP requets to DfE Sign-In API to get user access and organisation details
        UserAccess userAccess = await dfeSignInApiClient
            .GetUserInfo(appSettings.DsiServiceId, organisationId, userId);
        Organisation userOrganisation = await dfeSignInApiClient
            .GetUserOrganisation(userId, organisationId);

        if (userAccess is null)
        {
            eventLogging.TrackEvent(2502, "User log in unsuccessful - user not associated with GIAP service", userId, sessionId, hostEnvironment.ContentRootPath);
            ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "DfE-SignIn"));
            ctx.HttpContext.Response.Redirect(Routes.Application.UserWithNoRole);
            return;
        }

        AddRoleClaims(claims, userAccess, authenticatedUserInfo);
        AddOrganisationClaims(claims, userOrganisation, organisationId, authenticatedUserInfo);

        ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "DfE-SignIn"));

        // CREATE USER IF DOESN'T EXIST
        IUseCaseRequestOnly<CreateUserIfNotExistsRequest> createUserIfNotExistsUseCase = ctx.HttpContext.RequestServices
            .GetService<IUseCaseRequestOnly<CreateUserIfNotExistsRequest>>();
        await createUserIfNotExistsUseCase.HandleRequestAsync(new CreateUserIfNotExistsRequest(authenticatedUserInfo.UserId));

        // CHECK FOR UNREAD NEWS
        IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse> getUnreadUserNewsUseCase = ctx.HttpContext.RequestServices
            .GetService<IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse>>();
        GetUnreadUserNewsResponse getUnreadUserNewsResponse = await getUnreadUserNewsUseCase
                .HandleRequestAsync(new GetUnreadUserNewsRequest(authenticatedUserInfo.UserId));

        ISessionProvider sessionProvider = ctx.HttpContext.RequestServices.GetService<ISessionProvider>();
        if (getUnreadUserNewsResponse.HasUpdates)
            sessionProvider.SetSessionValue(SessionKeys.ShowNewsBannerKey, true);

        // UPDATE LAST LOGGED IN
        IUseCaseRequestOnly<UpdateLastLoggedInRequest> upsertUserUseCase = ctx.HttpContext.RequestServices
            .GetService<IUseCaseRequestOnly<UpdateLastLoggedInRequest>>();
        await upsertUserUseCase.HandleRequestAsync(new UpdateLastLoggedInRequest(authenticatedUserInfo.UserId, DateTime.UtcNow));

        LoggingEvent loggingEvent = CreateLoggingEvent(userId, userEmail, userGivenName, userSurname, ctx.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty, userOrganisation, organisationId, authenticatedUserInfo, sessionId);
        await userApiClient.CreateLoggingEvent(loggingEvent);
        eventLogging.TrackEvent(1120, "User log in successful", userId, sessionId, hostEnvironment.ContentRootPath);
    }

    private static void AddRoleClaims(List<Claim> claims, UserAccess userAccess, AuthenticatedUserInfo userInfo)
    {
        if (userAccess.Roles is not null && userAccess.Roles.Any())
        {
            claims.AddRange(userAccess.Roles.Select(role => new Claim(ClaimTypes.Role, role.Code)));

            userInfo.IsAdmin = userAccess.Roles.Any(x => x.Code == Roles.Admin);
            userInfo.IsApprover = userAccess.Roles.Any(x => x.Code == Roles.Approver);
            userInfo.IsUser = userAccess.Roles.Any(x => x.Code == Roles.User);
        }
        claims.Add(new Claim(CustomClaimTypes.IsAdmin, userInfo.IsAdmin.ToString()));
        claims.Add(new Claim(CustomClaimTypes.IsApprover, userInfo.IsApprover.ToString()));
        claims.Add(new Claim(CustomClaimTypes.IsUser, userInfo.IsUser.ToString()));
    }

    private static void AddOrganisationClaims(List<Claim> claims, Organisation org, string organisationId, AuthenticatedUserInfo userInfo)
    {
        claims.Add(new Claim(CustomClaimTypes.OrganisationId, organisationId));
        claims.Add(new Claim(CustomClaimTypes.OrganisationName, org?.Name ?? string.Empty));
        claims.Add(new Claim(CustomClaimTypes.OrganisationCategoryId, org?.Category?.Id ?? string.Empty));
        claims.Add(new Claim(CustomClaimTypes.OrganisationEstablishmentTypeId, org?.EstablishmentType?.Id ?? string.Empty));
        claims.Add(new Claim(CustomClaimTypes.OrganisationLowAge, org?.StatutoryLowAge ?? "0"));
        claims.Add(new Claim(CustomClaimTypes.OrganisationHighAge, org?.StatutoryHighAge ?? "0"));
        claims.Add(new Claim(CustomClaimTypes.EstablishmentNumber, org?.EstablishmentNumber ?? string.Empty));
        claims.Add(new Claim(CustomClaimTypes.LocalAuthorityNumber, org?.LocalAuthority?.Code ?? string.Empty));
        claims.Add(new Claim(CustomClaimTypes.UniqueReferenceNumber, org?.UniqueReferenceNumber ?? string.Empty));
        claims.Add(new Claim(CustomClaimTypes.UniqueIdentifier, org?.UniqueIdentifier ?? string.Empty));
        claims.Add(new Claim(CustomClaimTypes.UKProviderReferenceNumber, org?.UKProviderReferenceNumber ?? string.Empty));
    }


    private static LoggingEvent CreateLoggingEvent(
        string userId,
        string userEmail,
        string userGivenName,
        string userSurname,
        string userIpAddress,
        Organisation org,
        string organisationId,
        AuthenticatedUserInfo userInfo,
        string sessionId)
    {
        return new LoggingEvent
        {
            UserGuid = userId,
            UserEmail = userEmail,
            UserGivenName = userGivenName,
            UserSurname = userSurname,
            UserIpAddress = userIpAddress,
            OrganisationGuid = organisationId,
            OrganisationName = org?.Name ?? string.Empty,
            OrganisationCategoryID = org?.Category?.Id ?? string.Empty,
            OrganisationType = DSIUserHelper.GetOrganisationType(org?.Category?.Id ?? string.Empty),
            EstablishmentNumber = org?.EstablishmentNumber ?? string.Empty,
            LocalAuthorityNumber = org?.LocalAuthority?.Code ?? string.Empty,
            UKProviderReferenceNumber = org?.UKProviderReferenceNumber ?? string.Empty,
            UniqueReferenceNumber = org?.UniqueReferenceNumber ?? string.Empty,
            UniqueIdentifier = org?.UniqueIdentifier ?? string.Empty,
            GIAPUserRole = DSIUserHelper.GetGIAPUserRole(userInfo.IsAdmin, userInfo.IsApprover, userInfo.IsUser),
            ActionName = LogEventActionType.UserLoggedIn.ToString(),
            ActionDescription = LogEventActionType.UserLoggedIn.LogEventActionDescription(),
            SessionId = sessionId
        };
    }
}
