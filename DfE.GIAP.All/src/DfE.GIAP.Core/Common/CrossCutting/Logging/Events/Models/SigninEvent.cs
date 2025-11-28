namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

public record SigninEvent(
    string UserId,
    string SessionId,
    string Description,
    string OrgURN,
    string OrgName,
    string OrgCategory,
    SigninPayload Payload)
    : BusinessEvent<SigninPayload>(UserId, SessionId, Description, OrgURN, OrgName, OrgCategory, Payload)
{
    public override string EventName => nameof(SigninEvent);
}

public record SigninPayload() : IEventPayload;
