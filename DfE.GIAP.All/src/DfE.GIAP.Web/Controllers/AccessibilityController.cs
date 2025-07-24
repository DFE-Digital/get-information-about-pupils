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
        const string accessibilityPageKey = "Accessibility";

        GetContentByPageKeyUseCaseResponse response =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: accessibilityPageKey));

        return ToViewResult(accessibilityPageKey, response);
    }

    [HttpGet]
    public IActionResult Report()
    {
        return View();
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


internal sealed class GetContentByPageKeyResponseToAccessibilityViewModelMapper : IMapper<GetContentByPageKeyUseCaseResponse, AccessibilityViewModel>
{
    public AccessibilityViewModel Map(GetContentByPageKeyUseCaseResponse input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Content);
        return new AccessibilityViewModel()
        {
            Response = input.Content
        };
    }
}
