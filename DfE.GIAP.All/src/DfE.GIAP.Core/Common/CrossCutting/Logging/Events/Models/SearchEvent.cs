using System;
using System.Globalization;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

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

public record SearchPayload(
    SearchIdentifierType SearchIdentifierType,
    bool IsCustomSearch,
    Dictionary<string, bool> FilterFlags) : IEventPayload;

