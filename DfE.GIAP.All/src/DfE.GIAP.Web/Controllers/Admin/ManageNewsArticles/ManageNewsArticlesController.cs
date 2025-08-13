using System.Globalization;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Admin.ManageNewsArticles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DfE.GIAP.Web.Controllers.Admin.ManageNewsArticles;

[Route(Routes.Application.Admin)]
[Authorize(Roles = Roles.Admin)]
public class ManageNewsArticlesController : Controller
{
    private readonly IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> _getNewsArticleByIdUseCase;
    private readonly IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> _getNewsArticlesUseCase;
    private readonly IUseCaseRequestOnly<DeleteNewsArticleRequest> _deleteNewsArticleUseCase;
    private readonly IUseCaseRequestOnly<CreateNewsArticleRequest> _createNewsArticleUseCase;
    private readonly IUseCaseRequestOnly<UpdateNewsArticleRequest> _updateNewsArticleUseCase;
    private readonly ITextSanitiserInvoker _textSanitiserInvoker;

    public ManageNewsArticlesController(
        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> getNewsArticleByIdUseCase,
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> getNewsArticlesUseCase,
        IUseCaseRequestOnly<DeleteNewsArticleRequest> deleteNewsArticleUseCase,
        IUseCaseRequestOnly<CreateNewsArticleRequest> createNewsArticleUseCase,
        IUseCaseRequestOnly<UpdateNewsArticleRequest> updateNewsArticleUseCase,
        ITextSanitiserInvoker textSanitiserInvoker)
    {
        ArgumentNullException.ThrowIfNull(getNewsArticleByIdUseCase);
        _getNewsArticleByIdUseCase = getNewsArticleByIdUseCase;

        ArgumentNullException.ThrowIfNull(getNewsArticlesUseCase);
        _getNewsArticlesUseCase = getNewsArticlesUseCase;

        ArgumentNullException.ThrowIfNull(deleteNewsArticleUseCase);
        _deleteNewsArticleUseCase = deleteNewsArticleUseCase;

        ArgumentNullException.ThrowIfNull(createNewsArticleUseCase);
        _createNewsArticleUseCase = createNewsArticleUseCase;

        ArgumentNullException.ThrowIfNull(updateNewsArticleUseCase);
        _updateNewsArticleUseCase = updateNewsArticleUseCase;

        ArgumentNullException.ThrowIfNull(textSanitiserInvoker);
        _textSanitiserInvoker = textSanitiserInvoker;
    }

    [HttpGet]
    [Route("manage-news-articles")]
    public async Task<IActionResult> ManageNewsArticles()
    {
        GetNewsArticlesRequest request = new(NewsArticleSearchFilter.PublishedAndNotPublished);
        GetNewsArticlesResponse response = await _getNewsArticlesUseCase
            .HandleRequestAsync(request)
            .ConfigureAwait(false);

        List<Document> newsList = response.NewsArticles
            .Select(news => new Document
            {
                DocumentId = news.Id.Value,
                DocumentName = FormatNewsArticleName(news)
            })
            .ToList();

        return View("../Admin/ManageNewsArticles/ManageNewsArticles", new ManageNewsArticlesViewModel()
        {
            SelectedNewsId = string.Empty,
            NewsArticleList = new SelectList(newsList, nameof(Document.DocumentId), nameof(Document.DocumentName)),
            BackButton = new()
            {
                IsBackButtonEnabled = true,
                PreviousController = "Admin",
                PreviousAction = "Index"
            }
        });
    }

    [HttpPost]
    public IActionResult SelectNewsArticle(ManageNewsArticlesViewModel viewModel)
    {
        TempData["SelectedNewsId"] = viewModel.SelectedNewsId;
        return RedirectToAction("EditNewsArticle");
    }

    [HttpGet]
    [Route(Routes.ManageNewsArticles.CreateNewsArticle)]
    public IActionResult CreateNewsArticle()
    {
        return View("../Admin/ManageNewsArticles/CreateNewsArticle", new CreateNewsArticleViewModel
        {
            BackButton = new(
                isBackButtonEnabled: true,
                previousController: "ManageNewsArticles",
                previousAction: "ManageNewsArticles")
        });
    }

    [HttpPost]
    [Route(Routes.ManageNewsArticles.CreateNewsArticle)]
    public async Task<IActionResult> CreateNewsArticle(CreateNewsArticleViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("../Admin/ManageNewsArticles/CreateNewsArticle", viewModel);
        }

        NewsArticleViewModel userInputs = viewModel.NewsArticle;
        CreateNewsArticleRequest request = new(
            Title: userInputs.Title,
            Body: userInputs.Body,
            Published: userInputs.Published,
            Pinned: userInputs.Pinned);

        await _createNewsArticleUseCase.HandleRequestAsync(request);

        return View("../Admin/ManageNewsArticles/NewsArticleConfirmation", new ConfirmationViewModel
        {
            Title = Messages.NewsArticle.Success.CreateTitle,
            Body = Messages.NewsArticle.Success.CreateBody,
        });
    }

    [HttpPost]
    [Route(Routes.ManageNewsArticles.DeleteNewsArticle)]
    public async Task<IActionResult> DeleteNewsArticle(ManageNewsArticlesViewModel viewModel)
    {
        string articleId = viewModel.SelectedNewsId;

        DeleteNewsArticleRequest deleteRequest = new(NewsArticleIdentifier.From(articleId));
        await _deleteNewsArticleUseCase.HandleRequestAsync(deleteRequest);

        return View("../Admin/ManageNewsArticles/NewsArticleConfirmation", new ConfirmationViewModel
        {
            Title = Messages.NewsArticle.Success.DeleteTitle,
            Body = Messages.NewsArticle.Success.DeleteBody,
        });
    }

    [HttpGet]
    [Route(Routes.ManageNewsArticles.EditNewsAricle)]
    public async Task<IActionResult> EditNewsArticle()
    {
        if (!TempData.TryGetValue("SelectedNewsId", out object selectedNewsIdObj) || selectedNewsIdObj is null)
        {
            return RedirectToAction("ManageNewsArticles", "ManageNewsArticles");
        }

        string selectedNewsId = selectedNewsIdObj.ToString();
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedNewsId);

        GetNewsArticleByIdResponse response = await _getNewsArticleByIdUseCase.HandleRequestAsync(
            new GetNewsArticleByIdRequest(selectedNewsId));

        ArgumentNullException.ThrowIfNull(response.NewsArticle);

        return View("../Admin/ManageNewsArticles/EditNewsArticle", new EditNewsArticleViewModel()
        {
            SelectedNewsId = selectedNewsId,
            NewsArticle = new NewsArticleViewModel
            {
                Id = response.NewsArticle.Id.Value,
                Title = _textSanitiserInvoker.Sanitise(response.NewsArticle.Title).Value,
                Body = _textSanitiserInvoker.Sanitise(response.NewsArticle.Body).Value,
                Pinned = response.NewsArticle.Pinned,
                Published = response.NewsArticle.Published,
                CreatedDate = response.NewsArticle.CreatedDate,
                ModifiedDate = response.NewsArticle.ModifiedDate
            },
            BackButton = new BackButtonViewModel
            {
                IsBackButtonEnabled = true,
                PreviousController = "ManageNewsArticles",
                PreviousAction = "ManageNewsArticles"
            }
        });
    }

    [HttpPost]
    [Route(Routes.ManageNewsArticles.EditNewsAricle)]
    public async Task<IActionResult> UpdateNewsArticle(EditNewsArticleViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("../Admin/ManageNewsArticles/EditNewsArticle", viewModel);
        }

        UpdateNewsArticlesRequestProperties updateProperties = new(id: viewModel.NewsArticle.Id)
        {
            Title = _textSanitiserInvoker.Sanitise(viewModel.NewsArticle.Title),
            Body = _textSanitiserInvoker.Sanitise(viewModel.NewsArticle.Body),
            Pinned = viewModel.NewsArticle.Pinned,
            Published = viewModel.NewsArticle.Published,
        };

        await _updateNewsArticleUseCase.HandleRequestAsync(
            new UpdateNewsArticleRequest(updateProperties));

        return View("../Admin/ManageNewsArticles/NewsArticleConfirmation", new ConfirmationViewModel
        {
            Title = Messages.NewsArticle.Success.UpdateTitle,
            Body = Messages.NewsArticle.Success.UpdateBody,
        });
    }

    // Helper methods
    private static string FormatNewsArticleName(NewsArticle news)
    {
        string status = news.Published ? "Published" : "Draft";
        string pinned = news.Pinned ? " | Pinned" : string.Empty;
        string date = news.ModifiedDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("en-GB"));
        return $"{news.Title} | {date} | {status}{pinned}";
    }
}
