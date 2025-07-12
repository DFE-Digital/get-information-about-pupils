using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKey;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class TermsController : Controller
{
    private readonly IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse> _getContentByPageKeyUseCase;
    private readonly IMapper<GetContentByPageKeyResponse, TermsOfUseViewModel> _contentResponseToViewModelMapper;

    public TermsController(
        IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse> getContentByPageKeyUseCase,
        IMapper<GetContentByPageKeyResponse, TermsOfUseViewModel> contentResponseToViewModelMapper)
    {
        ArgumentNullException.ThrowIfNull(getContentByPageKeyUseCase);
        ArgumentNullException.ThrowIfNull(contentResponseToViewModelMapper);
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase;
        _contentResponseToViewModelMapper = contentResponseToViewModelMapper;
    }

    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        const string termOfUsePageKey = "TermOfUse";

        GetContentByPageKeyResponse response =
           await _getContentByPageKeyUseCase.HandleRequestAsync(
               new GetContentByPageKeyRequest(pageKey: termOfUsePageKey));

        return ToViewResult(termOfUsePageKey, response);
    }

    private ViewResult ToViewResult(string pageKey, GetContentByPageKeyResponse response)
    {
        if (response.Content == null)
        {
            throw new ArgumentException($"Unable to find content with key {pageKey}");
        }

        TermsOfUseViewModel model = _contentResponseToViewModelMapper.Map(response);
        return View(model);
    }
}


internal sealed class GetContentByPageKeyResponseToTermsOfUseViewModelMapper : IMapper<GetContentByPageKeyResponse, TermsOfUseViewModel>
{
    public TermsOfUseViewModel Map(GetContentByPageKeyResponse input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Content);
        return new TermsOfUseViewModel()
        {
            Response = input.Content
        };
    }
}
