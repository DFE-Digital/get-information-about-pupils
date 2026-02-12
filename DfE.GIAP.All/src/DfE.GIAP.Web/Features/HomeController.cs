using DfE.GIAP.Common.Constants;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.HostEnvironment;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
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
