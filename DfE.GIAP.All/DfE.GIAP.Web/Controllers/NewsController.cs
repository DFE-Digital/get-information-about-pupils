using DfE.GIAP.Common.Enums;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Service.News;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.Enums;

namespace DfE.GIAP.Web.Controllers;

[Route(Routes.Application.News)]
public class NewsController : Controller
{
    private readonly INewsService _newsService;
    private readonly IContentService _contentService;
    private readonly ILatestNewsBanner _newsBanner;
    private readonly IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> _getNewsArticlesUseCase;

    public NewsController(
        IContentService contentService,
        ILatestNewsBanner newsBanner,
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> getNewsArticleUseCase)
    {
        _contentService = contentService ??
            throw new ArgumentNullException(nameof(contentService));
        _newsBanner = newsBanner ??
            throw new ArgumentNullException(nameof(newsBanner));
        _getNewsArticlesUseCase = getNewsArticleUseCase ??
            throw new ArgumentNullException(nameof(getNewsArticleUseCase));
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        CommonResponseBody newsPublication = await _contentService.GetContent(DocumentType.PublicationSchedule).ConfigureAwait(false);
        CommonResponseBody newsMaintenance = await _contentService.GetContent(DocumentType.PlannedMaintenance).ConfigureAwait(false);

        GetNewsArticlesRequest request = new(NewsArticleSearchFilter.NotArchivedWithPublished);
        GetNewsArticlesResponse response = await _getNewsArticlesUseCase.HandleRequest(request).ConfigureAwait(false);

        NewsViewModel model = new()
        {
            NewsArticles = response.NewsArticles,
            NewsMaintenance = newsMaintenance,
            NewsPublication = newsPublication
        };
        await _newsBanner.RemoveLatestNewsStatus();
        return View(model);
    }

    [Route("archive")]
    public async Task<IActionResult> Archive()
    {
        GetNewsArticlesRequest request = new(NewsArticleSearchFilter.ArchivedWithPublished);
        GetNewsArticlesResponse response = await _getNewsArticlesUseCase.HandleRequest(request).ConfigureAwait(false);

        NewsViewModel model = new()
        {
            NewsArticles = response.NewsArticles
        };

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
