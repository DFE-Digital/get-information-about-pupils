using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class TermsController : Controller
{
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;
    private readonly IMapper<GetContentByPageKeyUseCaseResponse, TermsOfUseViewModel> _contentResponseToViewModelMapper;

    public TermsController(
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase,
        IMapper<GetContentByPageKeyUseCaseResponse, TermsOfUseViewModel> contentResponseToViewModelMapper)
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

        GetContentByPageKeyUseCaseResponse response =
           await _getContentByPageKeyUseCase.HandleRequestAsync(
               new GetContentByPageKeyUseCaseRequest(pageKey: termOfUsePageKey));

        return ToViewResult(termOfUsePageKey, response);
    }

    private ViewResult ToViewResult(string pageKey, GetContentByPageKeyUseCaseResponse response)
    {
        if (response.Content == null)
        {
            throw new ArgumentException($"Unable to find content with key {pageKey}");
        }

        TermsOfUseViewModel model = _contentResponseToViewModelMapper.Map(response);
        return View(model);
    }
}


internal sealed class GetContentByPageKeyResponseToTermsOfUseViewModelMapper : IMapper<GetContentByPageKeyUseCaseResponse, TermsOfUseViewModel>
{
    public TermsOfUseViewModel Map(GetContentByPageKeyUseCaseResponse input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Content);
        return new TermsOfUseViewModel()
        {
            Response = input.Content
        };
    }
}
