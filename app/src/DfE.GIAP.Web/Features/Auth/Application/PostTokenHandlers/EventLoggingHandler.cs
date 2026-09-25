using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Web.Extensions;

namespace DfE.GIAP.Web.Features.Auth.Application.PostTokenHandlers;

public class EventLoggingHandler : IPostTokenValidatedHandler
{
    private readonly IEventLogger _eventLogger;
    public EventLoggingHandler(IEventLogger eventLogger)
    {
        ArgumentNullException.ThrowIfNull(eventLogger);
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
    }
}
