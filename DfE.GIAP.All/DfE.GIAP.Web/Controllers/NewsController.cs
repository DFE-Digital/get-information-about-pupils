using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.News;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.News;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using DfE.GIAP.Core.Models.Common;
using System.Collections.Generic;
using DfE.GIAP.Web.Constants;

namespace DfE.GIAP.Web.Controllers;

[Route(Routes.Application.News)]
public class NewsController : Controller
{
    private readonly INewsService _newsService;
    private readonly IContentService _contentService;
    private readonly ILatestNewsBanner _newsBanner;

    public NewsController(INewsService newsService, IContentService contentService, ILatestNewsBanner newsBanner)
    {
        _newsService = newsService ??
            throw new ArgumentNullException(nameof(newsService));
        _contentService = contentService ??
            throw new ArgumentNullException(nameof(contentService));
        _newsBanner = newsBanner ??
            throw new ArgumentNullException(nameof(newsBanner));
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        CommonResponseBody newsPublication = await _contentService.GetContent(DocumentType.PublicationSchedule).ConfigureAwait(false);
        CommonResponseBody newsMaintenance = await _contentService.GetContent(DocumentType.PlannedMaintenance).ConfigureAwait(false);
        var model = new NewsViewModel
        {
            NewsMaintenance = newsMaintenance,
            NewsPublication = newsPublication
        };


        var requestBody = new RequestBody() { ARCHIVED = false, DRAFTS = false };
        List<Article> newsArticles = await _newsService.GetNewsArticles(requestBody).ConfigureAwait(false);

        model.Articles = newsArticles;


        await _newsBanner.RemoveLatestNewsStatus();
        return View(model);
    }

    [Route("archive")]
    public async Task<IActionResult> Archive()
    {
        var archivedArticles = await _newsService.GetNewsArticles(new RequestBody() { ARCHIVED = true, DRAFTS = true }).ConfigureAwait(false);
        var model = new NewsViewModel
        {
            Articles = archivedArticles
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
