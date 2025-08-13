using DfE.GIAP.Core.Common.Application;
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
    private readonly IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> _getNewsArticlesUseCase;

    public NewsController(
        ILatestNewsBanner newsBanner,
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> getNewsArticleUseCase)
    {
        ArgumentNullException.ThrowIfNull(newsBanner);
        ArgumentNullException.ThrowIfNull(getNewsArticleUseCase);
        _newsBanner = newsBanner;
        _getNewsArticlesUseCase = getNewsArticleUseCase;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        GetNewsArticlesRequest request = new(NewsArticleSearchFilter.Published);
        GetNewsArticlesResponse response = await _getNewsArticlesUseCase.HandleRequestAsync(request).ConfigureAwait(false);

        NewsViewModel model = new()
        {
            NewsArticles = response.NewsArticles,
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
