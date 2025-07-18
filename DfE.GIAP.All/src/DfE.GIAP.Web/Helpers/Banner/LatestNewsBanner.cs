using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;

namespace DfE.GIAP.Web.Helpers.Banner;

public class LatestNewsBanner : ILatestNewsBanner
{
    protected readonly ICommonService _commonService;
    private readonly HttpContext _httpContext;

    public LatestNewsBanner(ICommonService commonService, IHttpContextAccessor httpContextAccessor)
    {
        _commonService = commonService;
        _httpContext = httpContextAccessor.HttpContext;
    }

    /// <summary>
    /// Adds "showNewsBanner" to Session if user has not read or dismissed latest news or maintenance articles 
    /// </summary>
    /// <returns></returns>
    public async Task SetLatestNewsStatus()
    {
        string userId = _httpContext.User.GetUserId();
        bool result = await _commonService.GetLatestNewsStatus(userId).ConfigureAwait(false);
        if (result)
        {
            _httpContext.Session.SetString(SessionKeys.ShowNewsBanner, SessionKeys.ShowNewsBanner);
        }
    }

    /// <summary>
    /// Removes "showNewsBanner" from Session when user has accessed news page or clicked in "dismisse button"
    /// </summary>
    /// <returns></returns>
    public async Task RemoveLatestNewsStatus()
    {
        var userId = _httpContext.User.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.UserId).Value;
        var result = await _commonService.SetLatestNewsStatus(userId).ConfigureAwait(false);
        if (!result)
        {
            _httpContext.Session.Remove("showNewsBanner");
        }
    }
}
