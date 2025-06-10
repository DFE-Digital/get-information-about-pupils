using System;
using System.Threading.Tasks;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers.Landing;

[Route(Routes.Application.Landing)]
public class LandingController : Controller
{
    private readonly ILatestNewsBanner _newsBanner;
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;
    private readonly IMapper<GetContentByPageKeyUseCaseResponse, LandingViewModel> _contentResponstToViewModelMapper;

    public LandingController(
        ILatestNewsBanner newsBanner,
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase,
        IMapper<GetContentByPageKeyUseCaseResponse, LandingViewModel> contentResponstToViewModelMapper)
    {
        _newsBanner = newsBanner ??
            throw new ArgumentNullException(nameof(newsBanner));
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase ??
            throw new ArgumentNullException(nameof(getContentByPageKeyUseCase));
        _contentResponstToViewModelMapper = contentResponstToViewModelMapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await _newsBanner.SetLatestNewsStatus();

        GetContentByPageKeyUseCaseRequest request = new(pageKey: "LandingPage");
        GetContentByPageKeyUseCaseResponse response = await _getContentByPageKeyUseCase.HandleRequest(request);
        LandingViewModel model = _contentResponstToViewModelMapper.Map(response);
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
