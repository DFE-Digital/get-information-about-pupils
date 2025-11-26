namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

public class SigninEvent : BusinessEvent
{
    public SigninEvent(string userId, string sessionId, string description,
        string orgURN, string orgName, string orgCategory)
        : base(userId, sessionId, description, orgURN, orgName, orgCategory) { }

    public override string EventName => "SigninEvent";
}

