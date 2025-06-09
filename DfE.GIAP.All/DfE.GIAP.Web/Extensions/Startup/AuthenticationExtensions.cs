using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.DsiConfiguration;
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using DfE.GIAP.Web.Constants;

namespace DfE.GIAP.Web.Extensions.Startup;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddDsiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        AzureAppSettings appSettings = configuration.Get<AzureAppSettings>();

        var overallSessionTimeout = TimeSpan.FromMinutes(appSettings.SessionTimeout);
        var loggingEvent = new LoggingEvent();

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
                 options.LogoutPath = AppSettings.DsiLogoutPath;

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
                    AppSettings.DsiScopeOpenId,
                    AppSettings.DsiScopeEmail,
                    AppSettings.DsiScopeProfile,
                    AppSettings.DsiScopeOrganisationId
                ]);
                options.SaveTokens = true;
                options.CallbackPath = AppSettings.DsiCallbackPath;
                options.SignedOutCallbackPath = AppSettings.DsiSignedOutCallbackPath;
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
                        var isSpuriousAuthCbRequest =
                            context.Request.Path == options.CallbackPath &&
                        context.Request.Method == HttpMethods.Get &&
                            !context.Request.Query.ContainsKey("code");

                        if (isSpuriousAuthCbRequest)
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/");
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
                        var claims = new List<Claim>();
                        var sessionId = Guid.NewGuid().ToString();

                        ctx.Properties.IsPersistent = true;

                        ctx.Properties.ExpiresUtc = DateTime.UtcNow.Add(overallSessionTimeout);
                        var principal = ctx.Principal;

                        var organisation = JObject.Parse(ctx.Principal.FindFirst("Organisation").Value);
                        var organisationId = string.Empty;

                        var authenticatedUserInfo = new AuthenticatedUserInfo()
                        {
                            UserId = ctx.Principal.FindFirst("sub").Value
                        };

                        if (organisation.HasValues)
                        {

                            organisationId = organisation["id"].ToString();
                            var dfeSignInApiClient = ctx.HttpContext.RequestServices.GetService<IDfeSignInApiClient>();
                            var userAccess = await dfeSignInApiClient.GetUserInfo(appSettings.DsiServiceId, organisationId, authenticatedUserInfo.UserId);
                            var userOrganisation = await dfeSignInApiClient.GetUserOrganisation(authenticatedUserInfo.UserId, organisationId);
                            var userId = ctx.Principal.FindFirst("sub").Value;
                            var userEmail = ctx.Principal.FindFirst("email").Value;
                            var userGivenName = ctx.Principal.FindFirst("given_name").Value;
                            var userSurname = ctx.Principal.FindFirst("family_name").Value;
                            var userIpAddress = ctx.HttpContext.Connection.RemoteIpAddress.ToString();
                            var organisationCategoryId = userOrganisation?.Category?.Id ?? string.Empty;
                            var establishmentNumber = userOrganisation?.EstablishmentNumber ?? string.Empty;
                            var localAuthorityNumber = userOrganisation?.LocalAuthority?.Code ?? string.Empty;
                            var ukProviderReferenceNumber = userOrganisation?.UKProviderReferenceNumber ?? string.Empty;
                            var uniqueReferenceNumber = userOrganisation?.UniqueReferenceNumber ?? string.Empty;
                            var uniqueIdentifier = userOrganisation?.UniqueIdentifier ?? string.Empty;

                            var eventLogging = ctx.HttpContext.RequestServices.GetService<IEventLogging>();
                            var hostEnvironment = ctx.HttpContext.RequestServices.GetService<IHostEnvironment>();

                            /*Handles DSI users that aren't associated to the GIAP service (DSI returns a 404 response in this scenario when calling the GetUserInfo method)*/
                            if (userAccess == null)
                            {
                                eventLogging.TrackEvent(2502, $"User log in unsuccessful - user not associated with GIAP service", authenticatedUserInfo.UserId, sessionId, hostEnvironment.ContentRootPath);

                                claims.AddRange(new List<Claim>
                                {
                                    new Claim(CustomClaimTypes.UserId, userId),
                                    new Claim(CustomClaimTypes.SessionId, sessionId),
                                    new Claim(ClaimTypes.Email, userEmail),
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

                                    authenticatedUserInfo.IsAdmin = userAccess.Roles.Any(x => x.Code == Role.Admin);
                                    authenticatedUserInfo.IsApprover = userAccess.Roles.Any(x => x.Code == Role.Approver);
                                    authenticatedUserInfo.IsUser = userAccess.Roles.Any(x => x.Code == Role.User);
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

                            var userApiClient = ctx.HttpContext.RequestServices.GetService<ICommonService>();
                            var userUpdateResult = await userApiClient.CreateOrUpdateUserProfile(new UserProfile { UserId = authenticatedUserInfo.UserId },
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
