using System.Reflection;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

/// <summary>
/// Represents a business-related event that captures user activity and organizational context for auditing, analytics,
/// or workflow processing.
/// </summary>
/// <remarks>This abstract record serves as a base type for specific business event implementations. Derived types
/// should provide the event name and any additional event-specific properties. BusinessEvent instances are typically
/// used for logging, auditing, or triggering business processes.</remarks>
/// <param name="UserId">The unique identifier of the user associated with the event. Cannot be null or empty.</param>
/// <param name="SessionId">The identifier of the session in which the event occurred. Cannot be null or empty.</param>
/// <param name="Description">A descriptive message detailing the nature of the event. Cannot be null or empty.</param>
/// <param name="OrgURN">The Uniform Resource Name (URN) of the organization related to the event. Cannot be null or empty.</param>
/// <param name="OrgName">The display name of the organization related to the event. Cannot be null or empty.</param>
/// <param name="OrgCategory">The category or type of the organization related to the event. Cannot be null or empty.</param>
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

/// <summary>
/// Represents a business event with a strongly typed payload and associated user, session, and organization
/// information.
/// </summary>
/// <typeparam name="TPayload">The type of the event payload. Must implement <see cref="IEventPayload"/>.</typeparam>
/// <param name="UserId">The unique identifier of the user who triggered the event.</param>
/// <param name="SessionId">The identifier of the session in which the event occurred.</param>
/// <param name="Description">A description of the event, providing context or details about its occurrence.</param>
/// <param name="OrgURN">The Uniform Resource Name (URN) of the organization associated with the event.</param>
/// <param name="OrgName">The name of the organization associated with the event.</param>
/// <param name="OrgCategory">The category or type of the organization associated with the event.</param>
/// <param name="Payload">The strongly typed payload containing event-specific data.</param>
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
