using System;
using System.Threading.Tasks;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Helpers.HostEnvironment;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILatestNewsBanner _newsBanner;
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;
    public HomeController(
        ILatestNewsBanner newsBanner,
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase)
    {
        _newsBanner = newsBanner ??
           throw new ArgumentNullException(nameof(newsBanner));
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase ??
            throw new ArgumentNullException(nameof(getContentByPageKeyUseCase));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await _newsBanner.SetLatestNewsStatus();

        GetContentByPageKeyUseCaseResponse landingPageContentResponse =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: "Landing"));

        GetContentByPageKeyUseCaseResponse plannedMaintenanceContentResponse =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: "PlannedMaintenance"));

        GetContentByPageKeyUseCaseResponse publicationScheduleContentResponse =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: "PublicationSchedule"));

        GetContentByPageKeyUseCaseResponse frequentlyAskedQuestionsContentResponse =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: "FrequentlyAskedQuestions"));

        HomeViewModel model = new()
        {
            LandingResponse = landingPageContentResponse.Content,
            PlannedMaintenanceResponse = plannedMaintenanceContentResponse.Content,
            PublicationScheduleResponse = publicationScheduleContentResponse.Content,
            FAQResponse = frequentlyAskedQuestionsContentResponse.Content
        };

        return View(model);
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
