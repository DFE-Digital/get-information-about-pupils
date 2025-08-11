using DfE.GIAP.Common.Constants;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CheckNewsArticleUpdates;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Helpers.HostEnvironment;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILatestNewsBanner _newsBanner;
    private readonly IUseCase<CheckNewsArticleUpdatesRequest, CheckNewsArticleUpdateResponse> _checkNewsArticleUpdatesUseCase;
    private readonly ISessionProvider _sessionProvider;
    public HomeController(
        ILatestNewsBanner newsBanner
        //IUseCase<CheckNewsArticleUpdatesRequest, CheckNewsArticleUpdateResponse> checkNewsArticleUpdatesUseCase,
        /*ISessionProvider sessionProvider*/)
    {
        ArgumentNullException.ThrowIfNull(newsBanner);
        _newsBanner = newsBanner;

        //ArgumentNullException.ThrowIfNull(checkNewsArticleUpdatesUseCase);
        //_checkNewsArticleUpdatesUseCase = checkNewsArticleUpdatesUseCase;

        //ArgumentNullException.ThrowIfNull(sessionProvider);
        //_sessionProvider = sessionProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        //CheckNewsArticleUpdateResponse checkNewsArticleUpdatesResponse = await _checkNewsArticleUpdatesUseCase
        //    .HandleRequestAsync(new CheckNewsArticleUpdatesRequest(User.GetUserId()));

        //if (checkNewsArticleUpdatesResponse.HasUpdates)
        //    _sessionProvider.SetSessionValue("showNewsBanner", "showNewsBanner");

        await _newsBanner.SetLatestNewsStatus();
        return View();
    }

    [HttpPost]
    [ActionName("Index")]
    public IActionResult IndexPost()
    {
        if (User.IsOrganisationEstablishmentWithFurtherEducation())
        {
            return RedirectToAction(Global.FELearnerNumberSearchAction, Global.FELearnerNumberSearchController);
        }

        return RedirectToAction(Global.NPDLearnerNumberSearchAction, Global.NPDLearnerNumberSearchController);
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
        ErrorModel model = new();

        if (HostEnvironmentHelper.ShouldShowErrors())
        {
            model.ShowError = true;
            model.RequestId = HttpContext.TraceIdentifier;

            IExceptionHandlerPathFeature exceptionHandlerPathFeature =
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
