using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Consent;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers;

[Route(Routes.Application.Consent)]
public class ConsentController : Controller
{
    private readonly ICookieManager _cookieManager;
    private readonly AzureAppSettings _azureAppSettings;
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;

    public ConsentController(
        IOptions<AzureAppSettings> azureAppSettings,
        ICookieManager cookieManager,
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase)
    {
        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        ArgumentNullException.ThrowIfNull(cookieManager);
        ArgumentNullException.ThrowIfNull(getContentByPageKeyUseCase);
        _azureAppSettings = azureAppSettings.Value;
        _cookieManager = cookieManager;
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase;
    }


    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (_azureAppSettings.IsSessionIdStoredInCookie)
        {
            _cookieManager.Set(CookieKeys.GIAPSessionId, User.GetSessionId());
        }

        const string contentPageKey = "Consent";

        GetContentByPageKeyUseCaseResponse response =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: contentPageKey));

        if (response.Content is null)
        {
            throw new ArgumentException($"Could not find content for pageKey: {contentPageKey}");
        }

        ConsentViewModel viewModel = new()
        {
            Response = response.Content
        };

        return View(viewModel);
    }

    [AllowWithoutConsent]
    [HttpPost]
    public IActionResult Index(ConsentViewModel viewModel)
    {
        if (viewModel.ConsentGiven)
        {
            ConsentHelper.SetConsent(ControllerContext.HttpContext);
            return Redirect(Routes.Application.Home);
        }

        viewModel.ConsentError = true;
        return View(viewModel);
    }
}
