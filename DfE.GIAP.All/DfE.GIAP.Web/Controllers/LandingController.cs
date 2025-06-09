using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DfE.GIAP.Web.Constants;

namespace DfE.GIAP.Web.Controllers;

[Route(Routes.Application.Landing)]
public class LandingController : Controller
{
    private readonly IContentService _contentService;
    private readonly ILatestNewsBanner _newsBanner;

    public LandingController(IContentService contentService, ILatestNewsBanner newsBanner)
    {
        _contentService = contentService ??
            throw new ArgumentNullException(nameof(contentService));
        _newsBanner = newsBanner ??
            throw new ArgumentNullException(nameof(newsBanner));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await _newsBanner.SetLatestNewsStatus();

        LandingViewModel model = new()
        {
            LandingResponse = await _contentService.GetContent(DocumentType.Landing).ConfigureAwait(false),
            PlannedMaintenanceResponse = await _contentService.GetContent(DocumentType.PlannedMaintenance).ConfigureAwait(false),
            PublicationScheduleResponse = await _contentService.GetContent(DocumentType.PublicationSchedule).ConfigureAwait(false),
            FAQResponse = await _contentService.GetContent(DocumentType.FAQ).ConfigureAwait(false)
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
