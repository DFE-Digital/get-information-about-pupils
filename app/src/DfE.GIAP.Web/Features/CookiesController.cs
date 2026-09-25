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
    [ValidateAntiForgeryToken]
    [Route("CookiePreferences")]
    public IActionResult CookiePreferences(CookieUseViewModel viewModel)
    {
        _cookieProvider.Set(CookieKeys.GiapWebsiteUse, viewModel.CookieWebsiteUse);
        _cookieProvider.Set(CookieKeys.GiapComms, viewModel.CookieComms);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("CookieBanner")]
    public IActionResult CookieBanner(string analytics, string returnUrl)
    {
        ITrackingConsentFeature consent = HttpContext.Features.Get<ITrackingConsentFeature>();

        int expiryMinutes = (int)TimeSpan.FromDays(28).TotalMinutes;
        _cookieProvider.Set(
            key: CookieKeys.GiapCookieBannerSeen,
            value: "yes",
            expireTime: expiryMinutes,
            isEssential: true);

        bool accepted = analytics is "yes";
        if (accepted)
            consent?.GrantConsent();
        else
            consent?.WithdrawConsent();



        return string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl)
            ? RedirectToAction("Index", "Home")
            : LocalRedirect(returnUrl);
    }


    private bool IsCookieEnabled(string cookieName)
    {
        return Request?.Cookies[cookieName] == Global.StatusTrue;
    }
}
