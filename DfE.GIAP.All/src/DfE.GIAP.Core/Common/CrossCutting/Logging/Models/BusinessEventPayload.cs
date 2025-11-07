namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

public record BusinessEventPayload(
    string EventCategory,
    string EventAction,
    string EventStatus,
    string Description,
    string UserId,
    string OrgName,
    string OrgCategory,
    string OrgRole,
    string OrgPhaseOfEducation,
    string UserOrgNumericId,
    string SessionId,
    string ClientID, // ?
    Dictionary<string, object> Context);
