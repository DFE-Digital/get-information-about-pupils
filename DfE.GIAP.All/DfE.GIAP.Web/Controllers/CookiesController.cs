using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DfE.GIAP.Web.Controllers;

[AllowWithoutConsent]
public class CookiesController : Controller
{
    private readonly ICookieManager _cookieManager;

    public CookiesController(ICookieManager cookieManager)
    {
        _cookieManager = cookieManager ??
            throw new ArgumentNullException(nameof(cookieManager));
    }

    public IActionResult Index()
    {
        CookiePreferencesViewModel model = new()
        {
            CookieUse = new CookieUseViewModel
            {
                IsCookieWebsiteUse = IsCookieEnabled(Global.GiapWebsiteUse),
                IsCookieComms = IsCookieEnabled(Global.GiapComms)
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
            AppendCookie(Global.GiapWebsiteUse, viewModel.CookieWebsiteUse);
        }

        if (!string.IsNullOrEmpty(viewModel.CookieComms))
        {
            AppendCookie(Global.GiapComms, viewModel.CookieComms);
        }

        return RedirectToAction("Index", "Landing");
    }

    [HttpPost]
    [Route("AcceptCookies")]
    public IActionResult AcceptCookies([FromQuery] string returnUrl)
    {
        ITrackingConsentFeature consentTracker = HttpContext?.Features.Get<ITrackingConsentFeature>();
        consentTracker?.GrantConsent();

        var yearInMinutes = (int)(DateTime.Now.AddYears(1) - DateTime.Now).TotalMinutes;
        _cookieManager.Set(CookieKeys.AspConsentCookie, "yes", expireTime: yearInMinutes);

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
