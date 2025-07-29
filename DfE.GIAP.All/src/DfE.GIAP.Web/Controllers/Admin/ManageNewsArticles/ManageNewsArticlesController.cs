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
using DfE.GIAP.Web.ViewModels.Admin;
using DfE.GIAP.Web.ViewModels.Admin.ManageDocuments;
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
        ManageDocumentsViewModel model = new()
        {
            BackButton = new()
            {
                IsBackButtonEnabled = true,
                PreviousController = "Admin",
                PreviousAction = "Index"
            }
        };

        await LoadNewsArticles().ConfigureAwait(false);
        return View("../Admin/ManageNewsArticles/ManageNewsArticles", model);
    }

    [HttpGet]
    [Route(Routes.ManageNewsArticles.CreateNewsArticle)]
    public IActionResult CreateNewsArticle()
    {
        ManageDocumentsViewModel manageDocumentsModel = new()
        {
            BackButton = new(
                isBackButtonEnabled: true,
                previousController: "ManageDocuments",
                previousAction: "ManageDocuments")
        };

        return View("../Admin/ManageNewsArticles/CreateNewsArticle", manageDocumentsModel);
    }

    [HttpPost]
    [Route(Routes.ManageNewsArticles.CreateNewsArticle)]
    public async Task<IActionResult> CreateNewsArticle(ManageDocumentsViewModel manageDocumentsModel)
    {
        if (!ModelState.IsValid)
        {
            return View("../Admin/ManageNewsArticles/CreateNewsArticle", manageDocumentsModel);
        }

        CommonResponseBodyViewModel userInputs = manageDocumentsModel.DocumentData;
        CreateNewsArticleRequest request = new(
            Title: userInputs.Title,
            Body: userInputs.Body,
            Published: userInputs.Published,
            Pinned: userInputs.Pinned);

        await _createNewsArticleUseCase.HandleRequestAsync(request);

        ConfirmationViewModel model = new()
        {
            Title = Messages.NewsArticle.Success.CreateTitle,
            Body = Messages.NewsArticle.Success.CreateBody,
        };

        return View("../Admin/ManageNewsArticles/NewsArticleConfirmation", model);
    }

    [HttpPost]
    [Route(Routes.ManageNewsArticles.DeleteNewsArticle)]
    public async Task<IActionResult> DeleteNewsArticle(ManageDocumentsViewModel manageDocumentsModel)
    {
        string articleId = manageDocumentsModel.SelectedNewsId;

        DeleteNewsArticleRequest deleteRequest = new(NewsArticleIdentifier.From(articleId));
        await _deleteNewsArticleUseCase.HandleRequestAsync(deleteRequest);

        manageDocumentsModel.Confirmation = new Confirmation
        {
            Title = Messages.NewsArticle.Success.DeleteTitle,
            Body = Messages.NewsArticle.Success.DeleteBody,
        };

        return View("../Admin/ManageNewsArticles/Confirmation", manageDocumentsModel);
    }

    [HttpGet]
    [Route(Routes.ManageNewsArticles.EditNewsAricle)]
    public async Task<IActionResult> EditNewsArticle()
    {
        string selectedNewsId = TempData["SelectedNewsId"].ToString();

        if (string.IsNullOrEmpty(selectedNewsId))
            ArgumentException.ThrowIfNullOrEmpty(selectedNewsId);

        GetNewsArticleByIdResponse response = await _getNewsArticleByIdUseCase.HandleRequestAsync(
            new GetNewsArticleByIdRequest(selectedNewsId));

        ArgumentNullException.ThrowIfNull(response.NewsArticle);

        ManageDocumentsViewModel manageDocumentsModel = new()
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
            }
        };

        return View("../Admin/ManageNewsArticles/EditNewsArticle", manageDocumentsModel);
    }

    [HttpPost]
    [Route(Routes.ManageNewsArticles.EditNewsAricle)]
    public async Task<IActionResult> UpdateNewsArticle(ManageDocumentsViewModel manageDocumentsModel)
    {
        // TODO: Change to use specific view models, move away from "ManageDocumentsViewModel"
        if (!ModelState.IsValid)
        {
            return View("../Admin/ManageNewsArticles/EditNewsArticle", manageDocumentsModel);
        }

        UpdateNewsArticlesRequestProperties updateProperties = new(id: manageDocumentsModel.NewsArticle.Id)
        {
            Title = _textSanitiserInvoker.Sanitise(manageDocumentsModel.NewsArticle.Title),
            Body = _textSanitiserInvoker.Sanitise(manageDocumentsModel.NewsArticle.Body),
            Pinned = manageDocumentsModel.NewsArticle.Pinned,
            Published = manageDocumentsModel.NewsArticle.Published,
        };

        await _updateNewsArticleUseCase.HandleRequestAsync(
            new UpdateNewsArticleRequest(updateProperties));

        // Change to speciifc confirmation view model
        manageDocumentsModel.Confirmation = new()
        {
            Title = Messages.NewsArticle.Success.UpdateTitle,
            Body = Messages.NewsArticle.Success.UpdateBody,
        };

        ModelState.Clear();
        return View("../Admin/ManageNewsArticles/Confirmation", manageDocumentsModel);
    }

    [HttpPost]
    public IActionResult SelectNewsArticle(ManageDocumentsViewModel manageDocumentsModel)
    {
        if (string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId))
        {
            manageDocumentsModel.HasInvalidNewsList = true;
            ModelState.AddModelError("SelectNewsArticle", Messages.Common.Errors.NewsArticleRequired);
            return View("YourViewName", manageDocumentsModel);
        }

        TempData["SelectedNewsId"] = manageDocumentsModel.SelectedNewsId;
        return RedirectToAction("EditNewsArticle");
    }

    // Helper methods
    private async Task LoadNewsArticles()
    {
        GetNewsArticlesRequest request = new GetNewsArticlesRequest(NewsArticleSearchFilter.PublishedAndNotPublished);
        GetNewsArticlesResponse response = await _getNewsArticlesUseCase.HandleRequestAsync(request).ConfigureAwait(false);

        List<Document> newsList = response.NewsArticles
            .Select(news => new Document
            {
                DocumentId = news.Id.Value,
                DocumentName = FormatDocumentName(news)
            })
            .ToList();

        ViewBag.IsSuccess = newsList.Any();
        ViewBag.NewsDocuments = new SelectList(newsList, nameof(Document.DocumentId), nameof(Document.DocumentName));
    }

    private static string FormatDocumentName(NewsArticle news)
    {
        string status = news.Published ? "Published" : "Draft";
        string pinned = news.Pinned ? " | Pinned" : "";
        string date = news.ModifiedDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("en-GB"));
        return $"{news.Title} | {date} | {status}{pinned}";
    }
}
