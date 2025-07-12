using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKey;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class AccessibilityController : Controller
{
    private readonly IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse> _getContentByPageKeyUseCase;
    private readonly IMapper<GetContentByPageKeyResponse, AccessibilityViewModel> _contentResponseToViewModelMapper;

    public AccessibilityController(
        IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse> getContentByPageKeyUseCase,
        IMapper<GetContentByPageKeyResponse, AccessibilityViewModel> mapper)
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
        const string accessibilityPageKey = "Accessibility";

        GetContentByPageKeyResponse response =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyRequest(pageKey: accessibilityPageKey));

        return ToViewResult(accessibilityPageKey, response);
    }

    [HttpGet]
    public async Task<IActionResult> Report()
    {
        const string accessibilityReportPageKey = "AccessibilityReport";

        GetContentByPageKeyResponse response =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyRequest(pageKey: accessibilityReportPageKey));

        return ToViewResult(accessibilityReportPageKey, response);
    }

    private ViewResult ToViewResult(string pageKey, GetContentByPageKeyResponse response)
    {
        if (response.Content == null)
        {
            throw new ArgumentException($"Unable to find content with key {pageKey}");
        }

        AccessibilityViewModel model = _contentResponseToViewModelMapper.Map(response);
        return View(model);
    }
}


internal sealed class GetContentByPageKeyResponseToAccessibilityViewModelMapper : IMapper<GetContentByPageKeyResponse, AccessibilityViewModel>
{
    public AccessibilityViewModel Map(GetContentByPageKeyResponse input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Content);
        return new AccessibilityViewModel()
        {
            Response = input.Content
        };
    }
}
