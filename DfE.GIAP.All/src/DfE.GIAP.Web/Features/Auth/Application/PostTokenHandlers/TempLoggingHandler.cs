using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.LoggingEvent;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.DSIUser;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

namespace DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;

public class TempLoggingHandler : IPostTokenValidatedHandler
{
    private readonly ICommonService _userApiClient;
    private readonly IEventLogger _eventLogger;
    public TempLoggingHandler(ICommonService commonService, IEventLogger eventLogger)
    {
        ArgumentNullException.ThrowIfNull(commonService);
        ArgumentNullException.ThrowIfNull(eventLogger);
        _userApiClient = commonService;
        _eventLogger = eventLogger;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        string userId = context.Principal.GetUserId();
        string sessionId = context.Principal.GetSessionId();
        string orgUrn = context.Principal.GetUniqueReferenceNumber();
        string orgName = context.Principal.GetOrganisationName();
        string orgCategory = context.Principal.GetOrganisationCategoryID();

        _eventLogger.LogSignin(userId, sessionId, orgUrn, orgName, orgCategory);

        string userEmail = context.Principal.FindFirst("email")?.Value ?? string.Empty;
        string userGivenName = context.Principal.FindFirst("given_name")?.Value ?? string.Empty;
        string userSurname = context.Principal.FindFirst("family_name")?.Value ?? string.Empty;
        LoggingEvent loggingEvent = new()
        {
            UserGuid = userId,
            UserEmail = userEmail,
            UserGivenName = userGivenName,
            UserSurname = userSurname,
            UserIpAddress = string.Empty,
            OrganisationGuid = context.Principal.GetOrganisationId(),
            OrganisationName = context.Principal.GetOrganisationName() ?? string.Empty,
            OrganisationCategoryID = context.Principal.GetOrganisationCategoryID() ?? string.Empty,
            OrganisationType = DSIUserHelper.GetOrganisationType(context.Principal.GetOrganisationCategoryID()),
            EstablishmentNumber = context.Principal.GetEstablishmentNumber() ?? string.Empty,
            LocalAuthorityNumber = context.Principal.GetLocalAuthorityNumberForEstablishment() ?? string.Empty,
            UKProviderReferenceNumber = context.Principal.GetUKProviderReferenceNumber() ?? string.Empty,
            UniqueReferenceNumber = context.Principal.GetUniqueReferenceNumber() ?? string.Empty,
            UniqueIdentifier = context.Principal.GetUniqueIdentifier() ?? string.Empty,
            GIAPUserRole = context.Principal.GetUserRole(),
            ActionName = LogEventActionType.UserLoggedIn.ToString(),
            ActionDescription = LogEventActionType.UserLoggedIn.LogEventActionDescription(),
            SessionId = context.Principal.GetSessionId(),
        };

        await _userApiClient.CreateLoggingEvent(loggingEvent);
    }
}
