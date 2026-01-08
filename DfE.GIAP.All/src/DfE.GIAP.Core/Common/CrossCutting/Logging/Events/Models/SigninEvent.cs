namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

/// <summary>
/// Represents a business event that records a user sign-in action, including user, session, and organization details,
/// as well as additional payload information.
/// </summary>
/// <param name="UserId">The unique identifier of the user who performed the sign-in.</param>
/// <param name="SessionId">The identifier of the session associated with the sign-in event.</param>
/// <param name="Description">A descriptive message or details about the sign-in event.</param>
/// <param name="OrgURN">The Uniform Resource Name (URN) of the organization in which the sign-in occurred.</param>
/// <param name="OrgName">The display name of the organization associated with the sign-in.</param>
/// <param name="OrgCategory">The category or type of the organization related to the sign-in event.</param>
/// <param name="Payload">The payload containing additional data relevant to the sign-in event.</param>
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

/// <summary>
/// Represents the payload data for a sign-in event.
/// </summary>
/// <remarks>Use this type to encapsulate information associated with a sign-in operation when handling event
/// payloads. This record is typically used in event-driven systems to convey sign-in details to event handlers or
/// listeners.</remarks>
public record SigninPayload() : IEventPayload;
