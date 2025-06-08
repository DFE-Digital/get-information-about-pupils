using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Routes;
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
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.Common.Application;
using System.Linq;

namespace DfE.GIAP.Web.Controllers
{
    [Route(ApplicationRoute.News)]
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private readonly IContentService _contentService;
        private readonly ILatestNewsBanner _newsBanner;
        private readonly IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> _getNewsArticlesUseCase;
        public NewsController(INewsService newsService, IContentService contentService, ILatestNewsBanner newsBanner, IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> getNewsArticlesUseCase)
        {
            _newsService = newsService ??
                throw new ArgumentNullException(nameof(newsService));
            _contentService = contentService ??
                throw new ArgumentNullException(nameof(contentService));
            _newsBanner = newsBanner ??
                throw new ArgumentNullException(nameof(newsBanner));
            _getNewsArticlesUseCase = getNewsArticlesUseCase
                ?? throw new ArgumentNullException(nameof(getNewsArticlesUseCase));
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

            FilterNewsArticleRequest stateFilterRequest = FilterNewsArticleRequest.Published();
            GetNewsArticlesRequest request = new(stateFilterRequest);
            GetNewsArticlesResponse result = await _getNewsArticlesUseCase.HandleRequest(request).ConfigureAwait(false);

            model.Articles = result.NewsArticles.ToList();
            await _newsBanner.RemoveLatestNewsStatus();
            return View(model);
        }

        [Route("archive")]
        public async Task<IActionResult> Archive()
        {
            FilterNewsArticleRequest stateFilterRequest = FilterNewsArticleRequest.Archived();
            GetNewsArticlesRequest request = new(stateFilterRequest);
            GetNewsArticlesResponse result = await _getNewsArticlesUseCase.HandleRequest(request).ConfigureAwait(false);

            var model = new NewsViewModel
            {
                Articles = result.NewsArticles.ToList()
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
}
