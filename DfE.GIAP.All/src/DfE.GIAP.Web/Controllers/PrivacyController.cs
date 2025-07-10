using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKey;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class PrivacyController : Controller
{
    private readonly IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse> _getContentByPageKeyUseCase;
    private readonly IMapper<GetContentByPageKeyResponse, PrivacyViewModel> _contentResponseToViewModelMapper;

    public PrivacyController(
        IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse> getContentByPageKeyUseCase,
        IMapper<GetContentByPageKeyResponse, PrivacyViewModel> mapper)
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

        GetContentByPageKeyResponse response =
           await _getContentByPageKeyUseCase.HandleRequestAsync(
               new GetContentByPageKeyRequest(pageKey: PrivacyNoticePageKey));

        return ToViewResult(PrivacyNoticePageKey, response);
    }

    private ViewResult ToViewResult(string pageKey, GetContentByPageKeyResponse response)
    {
        if (response.Content == null)
        {
            throw new ArgumentException($"Unable to find content with key {pageKey}");
        }

        PrivacyViewModel model = _contentResponseToViewModelMapper.Map(response);
        return View(model);
    }
}

internal sealed class GetContentByPageKeyResponseToPrivacyViewModelMapper : IMapper<GetContentByPageKeyResponse, PrivacyViewModel>
{
    public PrivacyViewModel Map(GetContentByPageKeyResponse input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Content);
        return new PrivacyViewModel()
        {
            Response = input.Content
        };
    }
}
