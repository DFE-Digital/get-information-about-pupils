using DfE.GIAP.Common.Constants;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.Providers.Cookie;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features;

[AllowWithoutConsent]
public class CookiesController : Controller
{
    private readonly ICookieProvider _cookieProvider;

    public CookiesController(ICookieProvider cookieProvider)
    {
        ArgumentNullException.ThrowIfNull(cookieProvider);
        _cookieProvider = cookieProvider;
    }

    public IActionResult Index()
    {
        CookiePreferencesViewModel model = new()
        {
            CookieUse = new CookieUseViewModel
            {
                IsCookieWebsiteUse = IsCookieEnabled(CookieKeys.GiapWebsiteUse),
                IsCookieComms = IsCookieEnabled(CookieKeys.GiapComms)
            }
        };

        return View(model);
    }

    [HttpPost]
    [Route("CookiePreferences")]
    public IActionResult CookiePreferences(CookieUseViewModel viewModel)
    {
        if (!string.IsNullOrEmpty(viewModel.CookieWebsiteUse))
        {
            AppendCookie(CookieKeys.GiapWebsiteUse, viewModel.CookieWebsiteUse);
        }

        if (!string.IsNullOrEmpty(viewModel.CookieComms))
        {
            AppendCookie(CookieKeys.GiapComms, viewModel.CookieComms);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Route("AcceptCookies")]
    public IActionResult AcceptCookies([FromQuery] string returnUrl)
    {
        ITrackingConsentFeature consentTracker = HttpContext?.Features.Get<ITrackingConsentFeature>();
        consentTracker?.GrantConsent();

        int yearInMinutes = (int)(DateTime.Now.AddYears(1) - DateTime.Now).TotalMinutes;
        _cookieProvider.Set(CookieKeys.AspConsentCookie, "yes", expireTime: yearInMinutes);

        return Redirect(returnUrl);
    }

    private bool IsCookieEnabled(string cookieName)
    {
        return Request?.Cookies[cookieName] == Global.StatusTrue;
    }

    private void AppendCookie(string cookieName, string cookieValue)
    {
        CookieOptions options = new()
        {
            Expires = DateTime.Now.AddDays(28)
        };

        Response?.Cookies.Append(cookieName, cookieValue, options);
    }

}
