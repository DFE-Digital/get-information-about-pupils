using System.Reflection;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

public abstract record BusinessEvent(
    string UserId,
    string SessionId,
    string Description,
    string OrgURN,
    string OrgName,
    string OrgCategory)
{
    public abstract string EventName { get; }
    public abstract IDictionary<string, string> ToProperties();

    protected IDictionary<string, string> BaseProperties() => new Dictionary<string, string>
    {
        ["UserId"] = UserId,
        ["SessionId"] = SessionId,
        ["Description"] = Description,
        ["OrgURN"] = OrgURN,
        ["OrgName"] = OrgName,
        ["OrgCategory"] = OrgCategory

    };
}

public abstract record BusinessEvent<TPayload>(
    string UserId,
    string SessionId,
    string Description,
    string OrgURN,
    string OrgName,
    string OrgCategory,
    TPayload Payload) : BusinessEvent(UserId, SessionId, Description, OrgURN, OrgName, OrgCategory)
    where TPayload : IEventPayload
{
    public TPayload Properties => Payload;

    public override IDictionary<string, string> ToProperties()
    {
        IDictionary<string, string> props = BaseProperties();
        foreach (PropertyInfo p in typeof(TPayload).GetProperties())
        {
            props[p.Name] = p.GetValue(Payload)?.ToString() ?? string.Empty;
        }

        return props;
    }
}
