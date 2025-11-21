using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Web.Extensions;

namespace DfE.GIAP.Web.Features.Logging;

public class BusinessEventFactory : IBusinessEventFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public BusinessEventFactory(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public SearchEvent CreateSearch(bool isCustomSearch, string customTextSearch)
    {
        string userId = _httpContextAccessor.HttpContext.User.GetUserId();
        string sessionId = _httpContextAccessor.HttpContext.User.GetSessionId();
        string description = "User performed a search";
        string orgUrn = _httpContextAccessor.HttpContext.User.GetUniqueReferenceNumber();
        string orgName = _httpContextAccessor.HttpContext.User.GetOrganisationName();
        string orgCategory = _httpContextAccessor.HttpContext.User.GetOrganisationCategoryID();

        return new SearchEvent(userId, sessionId, description, orgUrn, orgName, orgCategory, isCustomSearch, customTextSearch);
    }

    public DownloadEvent CreateDownload(DownloadType downloadType, DownloadFileFormat downloadFormat, DownloadEventType? downloadEventType = null)
    {
        string userId = _httpContextAccessor.HttpContext.User.GetUserId();
        string sessionId = _httpContextAccessor.HttpContext.User.GetSessionId();
        string description = "User performed a download";
        string orgUrn = _httpContextAccessor.HttpContext.User.GetUniqueReferenceNumber();
        string orgName = _httpContextAccessor.HttpContext.User.GetOrganisationName();
        string orgCategory = _httpContextAccessor.HttpContext.User.GetOrganisationCategoryID();

        return new DownloadEvent(userId, sessionId, description, orgUrn, orgName, orgCategory, downloadType, downloadFormat, downloadEventType);
    }

    public SigninEvent CreateSignin()
    {
        string userId = _httpContextAccessor.HttpContext.User.GetUserId();
        string sessionId = _httpContextAccessor.HttpContext.User.GetSessionId();
        string description = "User logged in";
        string orgUrn = _httpContextAccessor.HttpContext.User.GetUniqueReferenceNumber();
        string orgName = _httpContextAccessor.HttpContext.User.GetOrganisationName();
        string orgCategory = _httpContextAccessor.HttpContext.User.GetOrganisationCategoryID();

        return new SigninEvent(userId, sessionId, description, orgUrn, orgName, orgCategory);
    }
}

