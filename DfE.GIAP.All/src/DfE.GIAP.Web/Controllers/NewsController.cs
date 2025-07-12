using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

[Route(Routes.Application.News)]
public class NewsController : Controller
{
    private readonly ILatestNewsBanner _newsBanner;
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;
    private readonly IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> _getNewsArticlesUseCase;

    public NewsController(
        ILatestNewsBanner newsBanner,
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> getNewsArticleUseCase,
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase)
    {
        ArgumentNullException.ThrowIfNull(newsBanner);
        ArgumentNullException.ThrowIfNull(getNewsArticleUseCase);
        ArgumentNullException.ThrowIfNull(getContentByPageKeyUseCase);
        _newsBanner = newsBanner;
        _getNewsArticlesUseCase = getNewsArticleUseCase;
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        GetContentByPageKeyUseCaseResponse publicationScheduleContentResponse =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: "PublicationSchedule"));

        GetContentByPageKeyUseCaseResponse plannedMaintenanceContentResponse =
            await _getContentByPageKeyUseCase.HandleRequestAsync(
                new GetContentByPageKeyUseCaseRequest(pageKey: "PlannedMaintenance"));

        GetNewsArticlesRequest request = new(NewsArticleSearchFilter.Published);
        GetNewsArticlesResponse response = await _getNewsArticlesUseCase.HandleRequestAsync(request).ConfigureAwait(false);

        NewsViewModel model = new()
        {
            NewsArticles = response.NewsArticles,
            NewsMaintenance = plannedMaintenanceContentResponse.Content,
            NewsPublication = publicationScheduleContentResponse.Content
        };

        await _newsBanner.RemoveLatestNewsStatus();
        return View(model);
    }

    [HttpGet]
    [Route("dismiss")]
    public async Task<IActionResult> DismissNewsBanner([FromQuery] string returnUrl)
    {
        await _newsBanner.RemoveLatestNewsStatus();
        return Redirect($"{returnUrl}?returnToSearch=true");
    }
}
