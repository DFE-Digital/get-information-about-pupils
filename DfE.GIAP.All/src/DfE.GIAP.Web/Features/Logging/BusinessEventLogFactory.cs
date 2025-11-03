using System.Security.Claims;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.Logging;

public class BusinessEventLogFactory : ILogEntryFactory<BusinessEventPayloadOptions, BusinessEventPayload>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BusinessEventLogFactory(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    public Log<BusinessEventPayload> Create(BusinessEventPayloadOptions options)
    {
        ClaimsPrincipal user = _httpContextAccessor.HttpContext?.User;
        Dictionary<string, object> context = options.Context ?? new();

        // Shared top-level properties
        string userId = user.GetUserId() ?? null;
        string sessionId = user?.GetSessionId() ?? null;
        string orgName = user?.GetOrganisationName() ?? null;
        string orgCategory = user?.GetOrganisationCategoryID() ?? null;
        string orgRole = user?.GetUserRole() ?? null;
        string orgPhase = null; // what is phase of education?
        string userOrgNumericId = user?.GetOrganisationId() ?? null;
        string clientId = context.GetValueOrDefault("ClientID")?.ToString() ?? null;

        // Event-specific enrichment
        switch (options.EventCategory)
        {
            case BusinessEventCategory.UserSignIn:
                break;
            case BusinessEventCategory.Search:
                break;
            case BusinessEventCategory.Download:
                break;
        }

        BusinessEventPayload payload = new BusinessEventPayload(
            EventCategory: options.EventCategory.ToString(),
            EventAction: options.EventAction,
            EventStatus: options.EventStatus,
            Description: options.Description,
            UserId: userId,
            OrgName: orgName,
            OrgCategory: orgCategory,
            OrgRole: orgRole,
            OrgPhaseOfEducation: orgPhase,
            UserOrgNumericId: userOrgNumericId,
            SessionId: sessionId,
            ClientID: clientId,
            Context: context);

        return new Log<BusinessEventPayload>
        {
            Payload = payload,
        };
    }
}
