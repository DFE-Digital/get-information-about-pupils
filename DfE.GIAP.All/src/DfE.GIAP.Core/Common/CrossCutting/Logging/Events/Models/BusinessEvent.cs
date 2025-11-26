namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

public abstract class BusinessEvent
{
    public string UserId { get; }
    public string SessionId { get; }
    public string Description { get; }
    public string OrgURN { get; }
    public string OrgName { get; }
    public string OrgCategory { get; }

    protected BusinessEvent(string userId, string sessionId, string description,
        string orgURN, string orgName, string orgCategory)
    {
        UserId = userId;
        SessionId = sessionId;
        Description = description;
        OrgURN = orgURN;
        OrgName = orgName;
        OrgCategory = orgCategory;
    }

    public abstract string EventName { get; }
    public virtual IDictionary<string, string> ToProperties()
    {
        return new Dictionary<string, string>
        {
            { "UserId", UserId },
            { "SessionId", SessionId },
            { "Description", Description },
            { "OrgURN", OrgURN },
            { "OrgName", OrgName },
            { "OrgCategory", OrgCategory }
        };
    }
}
