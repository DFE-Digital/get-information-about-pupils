using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.LoggingEvent;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.DsiApiClient;
using DfE.GIAP.Web.Helpers.DSIUser;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using System.Security.Claims;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using DfE.GIAP.Web.Constants;

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
            .AddCookie(options =>
             {
                 options.ExpireTimeSpan = overallSessionTimeout;
                 options.SlidingExpiration = true;
                 options.LogoutPath = DsiKeys.CallbackPaths.Logout;

                 options.Events.OnRedirectToAccessDenied = ctx =>
                 {
                     ctx.Response.StatusCode = 403;
                     ctx.Response.Redirect($"/{Routes.Application.UserWithNoRole}");
                     return Task.CompletedTask;
                 };
             })
            .AddOpenIdConnect(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.MetadataAddress = appSettings.DsiMetadataAddress;
                options.ClientId = appSettings.DsiClientId;
                options.ClientSecret = appSettings.DsiClientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.RequireHttpsMetadata = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Clear();
                options.Scope.AddRange(
                [
                    DsiKeys.Scope.OpenId,
                    DsiKeys.Scope.Email,
                    DsiKeys.Scope.Profile,
                    DsiKeys.Scope.OrganisationId
                ]);
                options.SaveTokens = true;
                options.CallbackPath = DsiKeys.CallbackPaths.Dsi;
                options.SignedOutCallbackPath = DsiKeys.CallbackPaths.SignedOut;
                options.TokenHandler = new JsonWebTokenHandler()
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

                    OnMessageReceived = context =>
                    {
                        bool isSpuriousAuthCbRequest =
                            context.Request.Path == options.CallbackPath &&
                        context.Request.Method == HttpMethods.Get &&
                            !context.Request.Query.ContainsKey("code");

                        if (isSpuriousAuthCbRequest)
                        {
                            context.HandleResponse();
                            context.Response.Redirect(Routes.Application.Home);
                        }

                        return Task.CompletedTask;
                    },


                    OnRemoteFailure = ctx =>
                    {
                        ctx.HandleResponse();
                        return Task.FromException(ctx.Failure);
                    },

                    OnTokenValidated = async ctx =>
                    {
                        List<Claim> claims = new();
                        string sessionId = Guid.NewGuid().ToString();

                        ctx.Properties.IsPersistent = true;
                        ctx.Properties.ExpiresUtc = DateTime.UtcNow.Add(overallSessionTimeout);

                        ClaimsPrincipal principal = ctx.Principal;
                        AuthenticatedUserInfo authenticatedUserInfo = new()
                        {
                            UserId = ctx.Principal.FindFirst("sub").Value
                        };

                        JObject organisation = JObject.Parse(ctx.Principal.FindFirst("Organisation").Value);
                        string organisationId = string.Empty;
                        if (organisation.HasValues)
                        {

                            organisationId = organisation["id"].ToString();
                            IDfeSignInApiClient dfeSignInApiClient = ctx.HttpContext.RequestServices.GetService<IDfeSignInApiClient>();
                            UserAccess userAccess = await dfeSignInApiClient.GetUserInfo(appSettings.DsiServiceId, organisationId, authenticatedUserInfo.UserId);
                            Organisation userOrganisation = await dfeSignInApiClient.GetUserOrganisation(authenticatedUserInfo.UserId, organisationId);
                            string userId = ctx.Principal.FindFirst("sub").Value;
                            string userEmail = ctx.Principal.FindFirst("email").Value;
                            string userGivenName = ctx.Principal.FindFirst("given_name").Value;
                            string userSurname = ctx.Principal.FindFirst("family_name").Value;
                            string userIpAddress = ctx.HttpContext.Connection.RemoteIpAddress.ToString();
                            string organisationCategoryId = userOrganisation?.Category?.Id ?? string.Empty;
                            string establishmentNumber = userOrganisation?.EstablishmentNumber ?? string.Empty;
                            string localAuthorityNumber = userOrganisation?.LocalAuthority?.Code ?? string.Empty;
                            string ukProviderReferenceNumber = userOrganisation?.UKProviderReferenceNumber ?? string.Empty;
                            string uniqueReferenceNumber = userOrganisation?.UniqueReferenceNumber ?? string.Empty;
                            string uniqueIdentifier = userOrganisation?.UniqueIdentifier ?? string.Empty;

                            IEventLogging eventLogging = ctx.HttpContext.RequestServices.GetService<IEventLogging>();
                            IHostEnvironment hostEnvironment = ctx.HttpContext.RequestServices.GetService<IHostEnvironment>();

                            /*Handles DSI users that aren't associated to the GIAP service (DSI returns a 404 response in this scenario when calling the GetUserInfo method)*/
                            if (userAccess == null)
                            {
                                eventLogging.TrackEvent(2502, $"User log in unsuccessful - user not associated with GIAP service", authenticatedUserInfo.UserId, sessionId, hostEnvironment.ContentRootPath);

                                claims.AddRange(new List<Claim>
                                {
                                    new(CustomClaimTypes.UserId, userId),
                                    new(CustomClaimTypes.SessionId, sessionId),
                                    new(ClaimTypes.Email, userEmail),
                                });

                                ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "DfE-SignIn"));
                                ctx.HttpContext.Response.Redirect(Routes.Application.UserWithNoRole);

                                return;
                            }
                            else
                            {

                                if (userAccess.Roles != null && userAccess.Roles.Any())
                                {
                                    claims.AddRange(userAccess.Roles.Select(role => new Claim(ClaimTypes.Role, role.Code)));

                                    authenticatedUserInfo.IsAdmin = userAccess.Roles.Any(x => x.Code == Roles.Admin);
                                    authenticatedUserInfo.IsApprover = userAccess.Roles.Any(x => x.Code == Roles.Approver);
                                    authenticatedUserInfo.IsUser = userAccess.Roles.Any(x => x.Code == Roles.User);
                                }

                                claims.AddRange(new List<Claim>
                                {
                                    new Claim(CustomClaimTypes.UserId, userId),
                                    new Claim(CustomClaimTypes.SessionId, sessionId),
                                    new Claim(ClaimTypes.GivenName, userGivenName),
                                    new Claim(ClaimTypes.Surname, userSurname),
                                    new Claim(ClaimTypes.Email, userEmail),
                                    new Claim(CustomClaimTypes.OrganisationId, organisationId),
                                    new Claim(CustomClaimTypes.OrganisationName, userOrganisation.Name),
                                    new Claim(CustomClaimTypes.OrganisationCategoryId, organisationCategoryId),
                                    new Claim(CustomClaimTypes.OrganisationEstablishmentTypeId, userOrganisation?.EstablishmentType?.Id ?? string.Empty),
                                    new Claim(CustomClaimTypes.OrganisationLowAge, userOrganisation?.StatutoryLowAge ?? "0"),
                                    new Claim(CustomClaimTypes.OrganisationHighAge, userOrganisation?.StatutoryHighAge ?? "0"),
                                    new Claim(CustomClaimTypes.EstablishmentNumber, establishmentNumber),
                                    new Claim(CustomClaimTypes.LocalAuthorityNumber, localAuthorityNumber ),
                                    new Claim(CustomClaimTypes.UniqueReferenceNumber, uniqueReferenceNumber ),
                                    new Claim(CustomClaimTypes.UniqueIdentifier, uniqueIdentifier),
                                    new Claim(CustomClaimTypes.UKProviderReferenceNumber,ukProviderReferenceNumber ),
                                    new Claim(CustomClaimTypes.IsAdmin, authenticatedUserInfo.IsAdmin.ToString()),
                                    new Claim(CustomClaimTypes.IsApprover, authenticatedUserInfo.IsApprover.ToString()),
                                    new Claim(CustomClaimTypes.IsUser, authenticatedUserInfo.IsUser.ToString())

                                });

                                loggingEvent = new LoggingEvent
                                {
                                    UserGuid = userId,
                                    UserEmail = userEmail,
                                    UserGivenName = userGivenName,
                                    UserSurname = userSurname,
                                    UserIpAddress = userIpAddress,
                                    OrganisationGuid = organisationId,
                                    OrganisationName = userOrganisation.Name,
                                    OrganisationCategoryID = organisationCategoryId,
                                    OrganisationType = DSIUserHelper.GetOrganisationType(organisationCategoryId),
                                    EstablishmentNumber = establishmentNumber,
                                    LocalAuthorityNumber = localAuthorityNumber,
                                    UKProviderReferenceNumber = ukProviderReferenceNumber,
                                    UniqueReferenceNumber = uniqueReferenceNumber,
                                    UniqueIdentifier = uniqueIdentifier,
                                    GIAPUserRole = DSIUserHelper.GetGIAPUserRole(authenticatedUserInfo.IsAdmin,
                                                                                 authenticatedUserInfo.IsApprover,
                                                                                 authenticatedUserInfo.IsUser),
                                    ActionName = LogEventActionType.UserLoggedIn.ToString(),
                                    ActionDescription = LogEventActionType.UserLoggedIn.LogEventActionDescription(),
                                    SessionId = sessionId
                                };

                            }


                            ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "DfE-SignIn"));

                            ICommonService userApiClient = ctx.HttpContext.RequestServices.GetService<ICommonService>();
                            bool userUpdateResult = await userApiClient.CreateOrUpdateUserProfile(new UserProfile { UserId = authenticatedUserInfo.UserId },
                                                                                                 new AzureFunctionHeaderDetails
                                                                                                 {
                                                                                                     ClientId = authenticatedUserInfo.UserId,
                                                                                                     SessionId = sessionId
                                                                                                 });

                            //Logging Event
                            _ = await userApiClient.CreateLoggingEvent(loggingEvent);
                            eventLogging.TrackEvent(1120, $"User log in successful", authenticatedUserInfo.UserId, sessionId, hostEnvironment.ContentRootPath);
                        }
                    }
                };
            }
        );

        return services;
    }
}
