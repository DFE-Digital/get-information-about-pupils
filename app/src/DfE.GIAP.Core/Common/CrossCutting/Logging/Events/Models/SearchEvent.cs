namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

/// <summary>
/// Represents a business event that captures details of a user-initiated search, including user, session, organization,
/// and search-specific payload information.
/// </summary>
/// <param name="UserId">The unique identifier of the user who performed the search.</param>
/// <param name="SessionId">The identifier for the session in which the search occurred.</param>
/// <param name="Description">A description of the search event, providing additional context or details.</param>
/// <param name="OrgURN">The Uniform Resource Name (URN) of the organization associated with the search.</param>
/// <param name="OrgName">The name of the organization associated with the search.</param>
/// <param name="OrgCategory">The category of the organization associated with the search.</param>
/// <param name="Payload">The payload containing search-specific details, such as identifiers, flags, and filter information.</param>
public record SearchEvent(
    string UserId,
    string SessionId,
    string Description,
    string OrgURN,
    string OrgName,
    string OrgCategory,
    SearchPayload Payload)
    : BusinessEvent<SearchPayload>(UserId, SessionId, Description, OrgURN, OrgName, OrgCategory, Payload)
{
    public override string EventName => nameof(SearchEvent);

    public override IDictionary<string, string> ToProperties()
    {
        IDictionary<string, string> props = BaseProperties();

        props["SearchIdentifierType"] = Payload.SearchIdentifierType.ToString();
        props["IsCustomSearch"] = Payload.IsCustomSearch.ToString();

        // Flatten filter flags into telemetry properties
        if (Payload.FilterFlags != null)
        {
            foreach (KeyValuePair<string, bool> kvp in Payload.FilterFlags)
            {
                string normalisedKey = kvp.Key.ToLowerInvariant();
                props[$"Filter.{normalisedKey}"] = kvp.Value.ToString();
            }
        }

        return props;
    }
}

/// <summary>
/// Represents the payload for a search event, including the identifier type, custom search flag, and filter options.
/// </summary>
/// <param name="SearchIdentifierType">The type of identifier used to perform the search. Determines how the search is scoped or matched.</param>
/// <param name="IsCustomSearch">Indicates whether the search is a custom search. Set to <see langword="true"/> for custom searches; otherwise, <see
/// langword="false"/>.</param>
/// <param name="FilterFlags">A dictionary containing filter options for the search, where each key is the filter name and the value indicates
/// whether the filter is enabled.</param>
public record SearchPayload(
    SearchIdentifierType SearchIdentifierType,
    bool IsCustomSearch,
    Dictionary<string, bool> FilterFlags) : IEventPayload;

