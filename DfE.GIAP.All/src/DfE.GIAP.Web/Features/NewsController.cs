using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features;

[Route(Routes.Application.News)]
public class NewsController : Controller
{
    private readonly ISessionProvider _sessionProvider;
    private readonly IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> _getNewsArticlesUseCase;

    public NewsController(
        ISessionProvider sessionProvider,
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> getNewsArticleUseCase)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(getNewsArticleUseCase);
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

        if (_sessionProvider.ContainsSessionKey(SessionKeys.ShowNewsBannerKey))
            _sessionProvider.SetSessionValue(SessionKeys.ShowNewsBannerKey, false);

        return View(model);
    }

    [HttpGet]
    [Route("dismiss")]
    public IActionResult DismissNewsBanner([FromQuery] string returnUrl)
    {
        if (_sessionProvider.ContainsSessionKey(SessionKeys.ShowNewsBannerKey))
            _sessionProvider.SetSessionValue(SessionKeys.ShowNewsBannerKey, false);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        else
            return RedirectToAction("Index", "Home");
    }
}
