using System;
using System.Threading.Tasks;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

[Route(Routes.Application.Landing)]
public class LandingController : Controller
{
    private readonly ILatestNewsBanner _newsBanner;
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;

    public LandingController(
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

        LandingViewModel model = new()
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
}
