using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class AccessibilityController : Controller
{
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;
    private readonly IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel> _contentResponseToViewModelMapper;

    public AccessibilityController(
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase,
        IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel> mapper)
    {
        ArgumentNullException.ThrowIfNull(getContentByPageKeyUseCase);
        ArgumentNullException.ThrowIfNull(mapper);
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase;
        _contentResponseToViewModelMapper = mapper;
    }


    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        const string accessibilityPageKey = "AccessibilityReport";

        GetContentByPageKeyUseCaseResponse response =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: accessibilityPageKey));

        return ToViewResult(accessibilityPageKey, response);
    }

    [HttpGet]
    public async Task<IActionResult> Report()
    {
        const string accessibilityReportPageKey = "AccessibilityReport";

        GetContentByPageKeyUseCaseResponse response =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: accessibilityReportPageKey));

        return ToViewResult(accessibilityReportPageKey, response);
    }

    private ViewResult ToViewResult(string pageKey, GetContentByPageKeyUseCaseResponse response)
    {
        if (response.Content == null)
        {
            throw new ArgumentException($"Unable to find content with key {pageKey}");
        }

        AccessibilityViewModel model = _contentResponseToViewModelMapper.Map(response);
        return View(model);
    }
}


internal sealed class ContentByPageKeyResponseToAccessibilityViewModelMapper : IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>
{
    public AccessibilityViewModel Map(GetContentByPageKeyUseCaseResponse input)
    {
        return new AccessibilityViewModel()
        {
            Response = input.Content
        };
    }
}
