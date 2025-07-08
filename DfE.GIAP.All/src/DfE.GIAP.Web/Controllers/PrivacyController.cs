using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class PrivacyController : Controller
{
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;
    private readonly IMapper<GetContentByPageKeyUseCaseResponse, PrivacyViewModel> _contentResponseToViewModelMapper;

    public PrivacyController(
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase,
        IMapper<GetContentByPageKeyUseCaseResponse, PrivacyViewModel> mapper)
    {
        ArgumentNullException.ThrowIfNull(getContentByPageKeyUseCase);
        ArgumentNullException.ThrowIfNull(mapper);
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase;
        _contentResponseToViewModelMapper = mapper;
    }

    [AllowWithoutConsent]
    public async Task<IActionResult> Index()
    {
        const string PrivacyNoticePageKey = "PrivacyNotice";

        GetContentByPageKeyUseCaseResponse response =
           await _getContentByPageKeyUseCase.HandleRequestAsync(
               new GetContentByPageKeyUseCaseRequest(pageKey: PrivacyNoticePageKey));

        return ToViewResult(PrivacyNoticePageKey, response);
    }

    private ViewResult ToViewResult(string pageKey, GetContentByPageKeyUseCaseResponse response)
    {
        if (response.Content == null)
        {
            throw new ArgumentException($"Unable to find content with key {pageKey}");
        }

        PrivacyViewModel model = _contentResponseToViewModelMapper.Map(response);
        return View(model);
    }
}

internal sealed class GetContentByPageKeyResponseToPrivacyViewModelMapper : IMapper<GetContentByPageKeyUseCaseResponse, PrivacyViewModel>
{
    public PrivacyViewModel Map(GetContentByPageKeyUseCaseResponse input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Content);
        return new PrivacyViewModel()
        {
            Response = input.Content
        };
    }
}
