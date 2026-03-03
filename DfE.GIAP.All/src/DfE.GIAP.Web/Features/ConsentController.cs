using DfE.GIAP.Web.Config;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.Providers.Cookie;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SessionOptions = DfE.GIAP.Web.Config.SessionOptions;

namespace DfE.GIAP.Web.Features;

[Route(Routes.Application.Consent)]
public class ConsentController : Controller
{
    private readonly ISessionProvider _sessionProvider;
    private readonly ICookieProvider _cookieProvider;
    private readonly SessionOptions _sessionOptions;

    public ConsentController(
        ISessionProvider sessionProvider,
        IOptions<SessionOptions> sessionOptions,
        ICookieProvider cookieProvider)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(sessionOptions);
        ArgumentNullException.ThrowIfNull(sessionOptions.Value);
        _sessionOptions = sessionOptions.Value;

        ArgumentNullException.ThrowIfNull(cookieProvider);
        _cookieProvider = cookieProvider;
    }


    [AllowWithoutConsent]
    [HttpGet]
    public IActionResult Index()
    {
        if (_sessionOptions.IsSessionIdStoredInCookie)
        {
            _cookieProvider.Set(CookieKeys.GiapSessionId, User.GetSessionId());
        }
        return View(new ConsentViewModel());
    }

    [AllowWithoutConsent]
    [HttpPost]
    public IActionResult Index(ConsentViewModel viewModel)
    {
        if (viewModel.ConsentGiven)
        {
            _sessionProvider.SetSessionValue(SessionKeys.ConsentGivenKey, true);
            return Redirect(Routes.Application.Home);
        }

        viewModel.HasError = true;
        return View(viewModel);
    }
}
