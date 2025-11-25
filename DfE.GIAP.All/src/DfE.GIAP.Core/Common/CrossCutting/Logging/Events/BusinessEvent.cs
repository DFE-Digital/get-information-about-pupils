namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

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




public enum DownloadType { Search, Prepared, Metadata }
public enum DownloadFileFormat { CSV, TAB, XML }
public enum DownloadEventType { NPD, FE, PP, CTF, Security }


public class DownloadEvent : BusinessEvent
{
    public DownloadType DownloadType { get; }
    public DownloadFileFormat DownloadFileFormat { get; }
    public DownloadEventType? DownloadEventType { get; }

    public DownloadEvent(string userId, string sessionId, string description, string orgURN, string orgName, string orgCategory,
                         DownloadType downloadType, DownloadFileFormat downloadFormat, DownloadEventType? downloadEventType = null)
        : base(userId, sessionId, description, orgURN, orgName, orgCategory)
    {
        DownloadType = downloadType;
        DownloadFileFormat = downloadFormat;
        DownloadEventType = downloadEventType;
    }

    public override string EventName => "DownloadEvent";
    public override IDictionary<string, string> ToProperties()
    {
        IDictionary<string, string> props = base.ToProperties();
        props["DownloadType"] = DownloadType.ToString();
        props["DownloadFileFormat"] = DownloadFileFormat.ToString();
        props["DownloadEventType"] = DownloadEventType.ToString() ?? string.Empty;
        return props;
    }
}

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

public class SigninEvent : BusinessEvent
{
    public SigninEvent(string userId, string sessionId, string description,
        string orgURN, string orgName, string orgCategory)
        : base(userId, sessionId, description, orgURN, orgName, orgCategory) { }

    public override string EventName => "SigninEvent";
}

