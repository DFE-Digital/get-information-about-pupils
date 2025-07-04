using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Consent;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers;

[Route(Routes.Application.Consent)]
public class ConsentController : Controller
{
    private readonly ICookieManager _cookieManager;
    private readonly AzureAppSettings _azureAppSettings;
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;
    private readonly IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel> _contentResponseToViewModelMapper;


    public ConsentController(
        IOptions<AzureAppSettings> azureAppSettings,
        ICookieManager cookieManager,
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase,
        IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel> contentResponseToViewModelMapper)
    {
        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        ArgumentNullException.ThrowIfNull(cookieManager);
        ArgumentNullException.ThrowIfNull(getContentByPageKeyUseCase);
        ArgumentNullException.ThrowIfNull(contentResponseToViewModelMapper);
        _azureAppSettings = azureAppSettings.Value;
        _cookieManager = cookieManager;
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase;
        _contentResponseToViewModelMapper = contentResponseToViewModelMapper;
    }


    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (_azureAppSettings.IsSessionIdStoredInCookie)
        {
            _cookieManager.Set(CookieKeys.GIAPSessionId, User.GetSessionId());
        }

        const string contentPageKey = "Consent";

        GetContentByPageKeyUseCaseResponse response =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: contentPageKey));

        return ToViewResult(contentPageKey, response);
    }

    [AllowWithoutConsent]
    [HttpPost]
    public IActionResult Index(ConsentViewModel viewModel)
    {
        if (viewModel.ConsentGiven)
        {
            ConsentHelper.SetConsent(ControllerContext.HttpContext);
            return Redirect(Routes.Application.Home);
        }

        viewModel.HasError = true;
        return View(viewModel);
    }

    private ViewResult ToViewResult(string pageKey, GetContentByPageKeyUseCaseResponse response)
    {
        if (response.Content == null)
        {
            throw new ArgumentException($"Unable to find content with key {pageKey}");
        }

        ConsentViewModel model = _contentResponseToViewModelMapper.Map(response);
        return View(model);
    }

}

internal sealed class GetContentByPageKeyResponseToConsentViewModelMapper : IMapper<GetContentByPageKeyUseCaseResponse, ConsentViewModel>
{
    public ConsentViewModel Map(GetContentByPageKeyUseCaseResponse input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Content);
        return new ConsentViewModel()
        {
            Response = input.Content
        };
    }
}

