using System;
using System.Threading.Tasks;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Service.Content;
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
    private readonly IContentService _contentService;
    private readonly ICookieManager _cookieManager;
    private readonly AzureAppSettings _azureAppSettings;

    public ConsentController(
        IOptions<AzureAppSettings> azureAppSettings,
        IContentService contentService,
        ICookieManager cookieManager)
    {
        _contentService = contentService ??
            throw new ArgumentNullException(nameof(contentService));
        _cookieManager = cookieManager ??
            throw new ArgumentNullException(nameof(cookieManager));
        _azureAppSettings = azureAppSettings?.Value;
    }


    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (_azureAppSettings.IsSessionIdStoredInCookie)
        {
            _cookieManager.Set(CookieKeys.GIAPSessionId, User.GetSessionId());
        }

        CommonResponseBody consentResponse = await _contentService.GetContent(DocumentType.Consent).ConfigureAwait(false);
        ConsentViewModel model = new()
        {
            Response = consentResponse
        };

        return View(model);
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
