using DfE.GIAP.Web.Config;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.Providers.Cookie;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features;

[Route(Routes.Application.Consent)]
public class ConsentController : Controller
{
    private readonly ISessionProvider _sessionProvider;
    private readonly ICookieProvider _cookieProvider;
    private readonly AzureAppSettings _azureAppSettings;

    public ConsentController(
        ISessionProvider sessionProvider,
        IOptions<AzureAppSettings> azureAppSettings,
        ICookieProvider cookieProvider)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        ArgumentNullException.ThrowIfNull(cookieProvider);
        _sessionProvider = sessionProvider;
        _azureAppSettings = azureAppSettings.Value;
        _cookieProvider = cookieProvider;
    }


    [AllowWithoutConsent]
    [HttpGet]
    public IActionResult Index()
    {
        if (_azureAppSettings.IsSessionIdStoredInCookie)
            _cookieProvider.Set(CookieKeys.GiapSessionId, User.GetSessionId());

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
