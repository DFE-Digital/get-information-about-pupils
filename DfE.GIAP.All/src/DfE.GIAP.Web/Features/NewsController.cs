using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
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
    private readonly ITextSanitiser _textSanitiser;
    private readonly IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> _getNewsArticlesUseCase;

    public NewsController(
        ISessionProvider sessionProvider,
        ITextSanitiser textSanitiser,
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> getNewsArticleUseCase)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(textSanitiser);
        _textSanitiser = textSanitiser;

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
            NewsArticles = response.NewsArticles.Select(n => new NewsArticleViewModel()
            {
                Id = n.Id.Value,
                Title = _textSanitiser.Sanitise(n.Title).Value,
                Body = _textSanitiser.Sanitise(n.Body).Value,
                Pinned = n.Pinned,
                Published = n.Published,
                CreatedDate = n.CreatedDate,
                ModifiedDate = n.ModifiedDate
            })
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
