namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

public class SearchEvent : BusinessEvent
{
    public bool IsCustomSearch { get; }
    public string CustomTextSearch { get; }

    public SearchEvent(string userId, string sessionId, string description,
        string orgURN, string orgName, string orgCategory, bool isCustomSearch, string customTextSearch)
        : base(userId, sessionId, description, orgURN, orgName, orgCategory)
    {
        IsCustomSearch = isCustomSearch;
        CustomTextSearch = customTextSearch;
    }

    public override string EventName => "SearchEvent";

    public override IDictionary<string, string> ToProperties()
    {
        IDictionary<string, string> props = base.ToProperties();
        props["IsCustomSearch"] = IsCustomSearch.ToString();
        props["CustomTextSearch"] = CustomTextSearch;
        return props;
    }
}

