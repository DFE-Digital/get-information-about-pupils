using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Common.Helpers.HostEnvironment;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Consent;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers;

public class HomeController : Controller
{
    private readonly IContentService _contentService;
    private readonly ICookieManager _cookieManager;
    private readonly AzureAppSettings _azureAppSettings;

    public HomeController(IOptions<AzureAppSettings> azureAppSettings, IContentService contentService, ICookieManager cookieManager)
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

        return View("Index", model);
    }

    [AllowWithoutConsent]
    [HttpPost]
    public IActionResult Index(ConsentViewModel viewModel)
    {
        if (viewModel.ConsentGiven)
        {
            ConsentHelper.SetConsent(ControllerContext.HttpContext);
            return RedirectToAction("Index", "Landing");
        }

        viewModel.ConsentError = true;
        return View("Index", viewModel);
    }

    [Route("/error/404")]
    public IActionResult Error404()
    {
        return View();
    }

    [Route("/error/{code:int}")]
    public IActionResult Error(int code)
    {
        return View(new ErrorModel());
    }

    [AllowAnonymous]
    [AllowWithoutConsent]
    [HttpGet]
    public IActionResult Exception()
    {
        var model = new ErrorModel();

        if (HostEnvironmentHelper.ShouldShowErrors())
        {
            model.ShowError = true;
            model.RequestId = HttpContext.TraceIdentifier;

            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            model.ExceptionMessage += exceptionHandlerPathFeature?.Error.Message;
            model.Stacktrace = exceptionHandlerPathFeature?.Error.StackTrace;

            if (exceptionHandlerPathFeature?.Path == "/")
            {
                model.ExceptionMessage ??= string.Empty;
                model.ExceptionMessage += " Page: Home.";
            }
        }

        return View("../Home/Error", model);
    }

    [AllowAnonymous]
    [AllowWithoutConsent]
    [HttpGet]
    [Route(Routes.Application.UserWithNoRole)]
    public IActionResult UserWithNoRole()
    {
        return View();
    }

}
