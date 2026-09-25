using System.Security.Claims;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;
using DfE.GIAP.Web.Extensions;

namespace DfE.GIAP.Web.Features.Logging;

public class BusinessEventFactory : IBusinessEventFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public BusinessEventFactory(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    public SearchEvent CreateSearch(SearchIdentifierType searchIdentifierType, bool isCustomSearch, Dictionary<string, bool> filterFlags)
    {
        ClaimsPrincipal user = _httpContextAccessor.HttpContext.User;
        SearchPayload payload = new(searchIdentifierType, isCustomSearch, filterFlags);

        return new SearchEvent(
            UserId: user.GetUserId(),
            SessionId: user.GetSessionId(),
            Description: "User performed a search",
            OrgURN: user.GetUniqueReferenceNumber(),
            OrgName: user.GetOrganisationName(),
            OrgCategory: user.GetOrganisationCategoryID(),
            Payload: payload);
    }

    public DownloadEvent CreateDownload(DownloadOperationType downloadType, DownloadFileFormat downloadFormat,
        DownloadEventType? downloadEventType = null, string? batchId = null, Dataset? dataset = null)
    {
        ClaimsPrincipal user = _httpContextAccessor.HttpContext.User;
        DownloadPayload payload = new(downloadType, downloadFormat, downloadEventType, batchId, dataset);

        return new DownloadEvent(
            UserId: user.GetUserId(),
            SessionId: user.GetSessionId(),
            Description: "User performed a download",
            OrgURN: user.GetUniqueReferenceNumber(),
            OrgName: user.GetOrganisationName(),
            OrgCategory: user.GetOrganisationCategoryID(),
            Payload: payload);
    }

    public SigninEvent CreateSignin(string userId, string sessionId, string orgUrn, string orgName, string orgCategory)
    {
        SigninPayload payload = new();

        return new SigninEvent(
            UserId: userId,
            SessionId: sessionId,
            Description: "User logged in",
            OrgURN: orgUrn,
            OrgName: orgName,
            OrgCategory: orgCategory,
            Payload: payload);
    }
}

