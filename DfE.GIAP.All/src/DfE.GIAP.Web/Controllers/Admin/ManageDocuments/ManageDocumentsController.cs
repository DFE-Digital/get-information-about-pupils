﻿using System.Globalization;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using DfE.GIAP.Core.Models;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Admin;
using DfE.GIAP.Web.ViewModels.Admin.ManageDocuments;
using DfE.GIAP.Web.ViewModels.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DfE.GIAP.Web.Controllers.Admin.ManageDocuments;

[Route(Routes.Application.Admin)]
[Authorize(Roles = Roles.Admin)]
public class ManageDocumentsController : Controller
{
    private readonly IContentService _contentService;
    private readonly IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> _getNewsArticleByIdUseCase;
    private readonly IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> _getNewsArticlesUseCase;
    private readonly IUseCaseRequestOnly<DeleteNewsArticleRequest> _deleteNewsArticleUseCase;
    private readonly IUseCaseRequestOnly<CreateNewsArticleRequest> _createNewsArticleUseCase;
    private readonly IUseCaseRequestOnly<UpdateNewsArticleRequest> _updateNewsArticleUseCase;
    private readonly ITextSanitiserInvoker _textSanitiserInvoker;

    public ManageDocumentsController(
        IContentService contentService,
        IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse> getNewsArticleByIdUseCase,
        IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse> getNewsArticlesUseCase,
        IUseCaseRequestOnly<DeleteNewsArticleRequest> deleteNewsArticleUseCase,
        IUseCaseRequestOnly<CreateNewsArticleRequest> createNewsArticleUseCase,
        IUseCaseRequestOnly<UpdateNewsArticleRequest> updateNewsArticleUseCase,
        ITextSanitiserInvoker textSanitiserInvoker)
    {
        ArgumentNullException.ThrowIfNull(contentService);
        _contentService = contentService;

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
    [Route(Routes.ManageDocument.ManageDocuments)]
    public async Task<IActionResult> ManageDocuments(string docType, string docAction, string newsArticleId)
    {
        LoadDocumentsList();
        ViewBag.DisplayEditor = false;
        ManageDocumentsViewModel model = new()
        {
            BackButton = new()
            {
                IsBackButtonEnabled = true,
                PreviousController = "Admin",
                PreviousAction = "Index"
            }
        };

        bool isValidUrlAction = CheckValidUrlAction(docAction);
        bool isValidUrlDocType = CheckValidUrlDocType(docType);

        if (isValidUrlAction && isValidUrlDocType)
        {
            ViewBag.DisplayEditor = isValidUrlDocType;

            Enum.TryParse(docAction, out EditorActions action);

            switch (action)
            {
                case EditorActions.add:
                    return View("../Admin/ManageDocuments/CreateNewsArticle", model);

                case EditorActions.edit:
                    if (docType == DocumentType.Article.ToString())
                    {
                        await LoadNewsList().ConfigureAwait(false);
                        model.DocumentList = new Document { DocumentId = DocumentType.Article.ToString(), DocumentName = DocumentType.Article.ToString() };
                        model.SelectedNewsId = newsArticleId;
                        model.DocumentData = await GetSelectedNewsDocumentData(newsArticleId).ConfigureAwait(false);
                    }
                    else
                    {
                        model.DocumentList = new Document { DocumentId = docType };
                        model.DocumentData = await GetSelectedDocumentData(docType).ConfigureAwait(false);
                    }
                    return View("../Admin/ManageDocuments/ManageDocuments", model);

                default:
                    break;
            }
        }
        return View("../Admin/ManageDocuments/ManageDocuments", model);
    }

    [HttpPost]
    [Route(Routes.ManageDocument.ManageDocuments)]
    public async Task<IActionResult> ManageDocuments(ManageDocumentsViewModel manageDocumentsModel, string discard, string edit)
    {
        LoadDocumentsList();
        manageDocumentsModel.BackButton = new()
        {
            IsBackButtonEnabled = true,
            PreviousController = "Admin",
            PreviousAction = "Index"
        };

        if (manageDocumentsModel.DocumentList != null)
        {
            if (manageDocumentsModel.DocumentList.DocumentId == null)
            {
                ModelState.AddModelError("Document.Id", Messages.Common.Errors.DocumentRequired);
                manageDocumentsModel.HasInvalidDocumentList = true;
            }
            else
            {
                if (manageDocumentsModel.DocumentList.DocumentId != DocumentType.Article.ToString())
                {
                    // TODO: We don't care if selectedNewsId if it's not an artcile document type
                    manageDocumentsModel.SelectedNewsId = string.Empty;
                }

                if (manageDocumentsModel.DocumentList.DocumentId == DocumentType.Article.ToString() &&
                            string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId))
                {
                    await LoadNewsList().ConfigureAwait(false);
                }
                else
                {
                    ViewBag.DisplayEditor = true;
                    if (!string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId))
                    {
                        await LoadNewsList().ConfigureAwait(false);
                    }

                    if (string.IsNullOrEmpty(edit))
                    {
                        manageDocumentsModel.DocumentData = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId) ?
                                                                await GetSelectedNewsDocumentData(manageDocumentsModel.SelectedNewsId) :
                                                                await GetSelectedDocumentData(manageDocumentsModel.DocumentList.DocumentId);
                        ModelState.Clear();
                    }
                }
            }
        }
        else if (!string.IsNullOrEmpty(discard))
        {
            ViewBag.DisplayEditor = false;
            ModelState.Clear();
        }

        return View("../Admin/ManageDocuments/ManageDocuments", manageDocumentsModel);
    }

    [HttpGet]
    [Route(Routes.ManageDocument.CreateNewsArticle)]
    public IActionResult CreateNewsArticle()
    {
        ManageDocumentsViewModel manageDocumentsModel = new()
        {
            BackButton = new(
                isBackButtonEnabled: true,
                previousController: "ManageDocuments",
                previousAction: "ManageDocuments")
        };

        return View("../Admin/ManageDocuments/CreateNewsArticle", manageDocumentsModel);
    }

    [HttpPost]
    [Route(Routes.ManageDocument.CreateNewsArticle)]
    public async Task<IActionResult> CreateNewsArticle(ManageDocumentsViewModel manageDocumentsModel)
    {
        if (!ModelState.IsValid)
        {
            return View("../Admin/ManageDocuments/CreateNewsArticle", manageDocumentsModel);
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

        return View("../Admin/ManageDocuments/NewsArticleConfirmation", model);
    }

    [HttpPost]
    [Route(Routes.ManageDocument.DeleteNewsArticle)]
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

        return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
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

    [HttpGet]
    [Route(Routes.ManageDocument.EditNewsAricle)]
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

        return View("../Admin/ManageDocuments/EditNewsArticle", manageDocumentsModel);
    }

    [HttpPost]
    [Route(Routes.ManageDocument.EditNewsAricle)]
    public async Task<IActionResult> UpdateNewsArticle(ManageDocumentsViewModel manageDocumentsModel)
    {
        // TODO: Change to use specific view models, move away from "ManageDocumentsViewModel"
        if (!ModelState.IsValid)
        {
            return View("../Admin/ManageDocuments/EditNewsArticle", manageDocumentsModel);
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
        return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
    }



    // Document methods
    [HttpPost]
    [Route(Routes.ManageDocument.ManageDocumentsPreview)]
    public async Task<IActionResult> PreviewChanges(ManageDocumentsViewModel manageDocumentsModel, string preview)
    {
        if (ModelState.IsValid)
        {
            var updatedResult = await SetPinned(manageDocumentsModel);

            if (updatedResult is null)
            {
                return await GenerateErrorView(Messages.NewsArticle.Errors.UpdatedError).ConfigureAwait(false);
            }

            ViewBag.DisplayEditor = true;
            return View("../Admin/ManageDocuments/PreviewChanges", manageDocumentsModel);
        }

        LoadDocumentsList();
        ViewBag.DisplayEditor = true;
        manageDocumentsModel.DocumentData = await GetSelectedDocumentData(manageDocumentsModel.DocumentList.DocumentId);

        return View("../Admin/ManageDocuments/ManageDocuments", manageDocumentsModel);
    }

    [HttpPost]
    [Route(Routes.ManageDocument.ManageDocumentsPublish)]
    public async Task<IActionResult> PublishChanges(ManageDocumentsViewModel manageDocumentsModel, string publish)
    {
        if (manageDocumentsModel != null)
        {
            var draftResult = await SaveDraft(manageDocumentsModel).ConfigureAwait(false);

            if (draftResult is null)
            {
                var userErrorMessage = GenerateErrorMessage(manageDocumentsModel);
                return await GenerateErrorView(userErrorMessage);
            }

            var publishedDocument = await Publish(manageDocumentsModel).ConfigureAwait(false);

            if (publishedDocument is null)
            {
                var userErrorMessage = GenerateErrorMessage(manageDocumentsModel);
                return await GenerateErrorView(userErrorMessage);
            }
        }

        manageDocumentsModel.Confirmation = GenerateConfirmationMessage(manageDocumentsModel);

        return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
    }

    private async Task<CommonResponseBodyViewModel> SetPinned(ManageDocumentsViewModel manageDocumentsModel)
    {
        // TODO: Check not needed as this will never be a news article going forward
        var isNewsArticle = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId);

        var responseToPublish = new CommonRequestBody
        {
            Title = manageDocumentsModel.DocumentData.Title,
            Body = manageDocumentsModel.DocumentData.Body,
            UserAccount = Global.UserAccount,
            Username = Global.UserName,
            Id = isNewsArticle ? manageDocumentsModel.SelectedNewsId : manageDocumentsModel.DocumentData.Id,
            DocType = isNewsArticle ? (int)UpdateDocumentType.NewsArticles : (int)UpdateDocumentType.Content,
            Published = true,
            Action = manageDocumentsModel.DocumentData.Pinned ? (int)ActionTypes.Pinned : (int)ActionTypes.Unpinned
        };

        var results = await _contentService.SetDocumentToPublished(responseToPublish, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

        if (results is null) return default;

        return results.ConvertToViewModel();
    }

    private async Task<CommonResponseBodyViewModel> Publish(ManageDocumentsViewModel manageDocumentsModel)
    {
        var isNewsArticle = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId);

        var updatedResult = await SetPinned(manageDocumentsModel);
        if (updatedResult is null) return default;

        var responseToPublish = new CommonRequestBody
        {
            UserAccount = Global.UserAccount,
            Username = Global.UserName,
            Id = isNewsArticle ? manageDocumentsModel.SelectedNewsId : manageDocumentsModel.DocumentData.Id,
            DocType = isNewsArticle ? (int)UpdateDocumentType.NewsArticles : (int)UpdateDocumentType.Content,
            Published = true,
            Pinned = manageDocumentsModel.DocumentData.Pinned,
            Action = (int)ActionTypes.Publish
        };

        var results = await _contentService.SetDocumentToPublished(responseToPublish, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

        if (results is null) return default;

        return results.ConvertToViewModel();
    }

    private async Task<CommonResponseBodyViewModel> SaveDraft(ManageDocumentsViewModel manageDocumentsModel)
    {
        var isNewsArticle = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId);

        var updatedResult = await SetPinned(manageDocumentsModel);
        if (updatedResult is null) return default;

        var responseToSave = new CommonRequestBody
        {
            Title = manageDocumentsModel.DocumentData.Title,
            Body = manageDocumentsModel.DocumentData.Body,
            Pinned = manageDocumentsModel.DocumentData.Pinned,
            UserAccount = Global.UserAccount,
            Username = Global.UserName,
            Id = isNewsArticle ? manageDocumentsModel.SelectedNewsId : manageDocumentsModel.DocumentData.Id,
            DocType = isNewsArticle ? (int)UpdateDocumentType.NewsArticles : (int)UpdateDocumentType.Content,
            Published = false,
        };

        var results = await _contentService.AddOrUpdateDocument(responseToSave, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

        if (results is null) return default;

        return results.ConvertToViewModel();
    }

    private async Task<CommonResponseBodyViewModel> GetSelectedDocumentData(string documentName)
    {
        CommonResponseBodyViewModel commonResponseBodyViewModel = new CommonResponseBodyViewModel();

        Enum.TryParse(documentName, out DocumentType documentType);

        var commonResponseBody = await _contentService.GetContent(documentType);
        if (commonResponseBody != null)
        {
            commonResponseBodyViewModel = commonResponseBody.ConvertToViewModel();
        }

        return commonResponseBodyViewModel;
    }

    private async Task<CommonResponseBodyViewModel> GetSelectedNewsDocumentData(string newsArticleId)
    {
        CommonResponseBodyViewModel output = new();
        GetNewsArticleByIdRequest getNewsArticleByIdRequest = new(newsArticleId);

        GetNewsArticleByIdResponse response = await _getNewsArticleByIdUseCase.HandleRequestAsync(getNewsArticleByIdRequest);
        NewsArticle? responseArticle = response.NewsArticle;

        if (responseArticle is not null)
        {
            output.Id = responseArticle.Id.Value;
            output.Title = SecurityHelper.SanitizeText(responseArticle.Title);
            output.Body = SecurityHelper.SanitizeText(responseArticle.Body);
            output.Pinned = responseArticle.Pinned;
            output.Published = responseArticle.Published;
        }
        return output;
    }


    private void LoadDocumentsList()
    {
        var documentTypes = Enum
            .GetValues(typeof(DocumentType))
            .Cast<DocumentType>()
            .Select(dt => new
            {
                DocumentId = dt.ToString(),
                DocumentName = dt.GetDescription()
            })
            .OrderBy(x => x.DocumentName)
            .ToList();

        ViewBag.ListOfDocuments = new SelectList(documentTypes, "DocumentId", "DocumentName");
    }

    private async Task LoadNewsList()
    {
        GetNewsArticlesRequest request = new(NewsArticleSearchFilter.PublishedAndNotPublished);
        GetNewsArticlesResponse response = await _getNewsArticlesUseCase.HandleRequestAsync(request).ConfigureAwait(false);

        List<Document> newsList = new();
        foreach (NewsArticle news in response.NewsArticles)
        {
            string status = news.Published ? "Published" : "Draft";
            string pinned = news.Pinned ? " | Pinned" : "";
            string date = news.ModifiedDate.ToString("dd/MM/yyyy", new CultureInfo("en-GB"));
            string name = news.Title;

            newsList.Add(new Document
            {
                DocumentName = $"{name} | {date} | {status} {pinned}",
                DocumentId = news.Id.Value,
            });
        }

        ViewBag.IsSuccess = newsList.Count > 0 ? true : false;
        ViewBag.NewsDocuments = new SelectList(newsList, "DocumentId", "DocumentName");
    }


    private Confirmation GenerateConfirmationMessage(ManageDocumentsViewModel model)
    {
        string title = string.Empty, body = string.Empty;

        if (!string.IsNullOrEmpty(model.SelectedNewsId) && model.DocumentData.Id != null)
        {
            title = Messages.NewsArticle.Success.UpdateTitle;
            body = Messages.NewsArticle.Success.UpdateBody;
        }
        else if (model.DocumentData.Id != null && string.IsNullOrEmpty(model.SelectedNewsId))
        {
            title = Messages.NewsArticle.Success.DocumentUpdatedTitle;
            body = Messages.NewsArticle.Success.DocumentUpdatedBody;
        }
        else if (model.DocumentData.Id == null && !string.IsNullOrEmpty(model.SelectedNewsId))
        {
            title = Messages.NewsArticle.Success.CreateTitle;
            body = Messages.NewsArticle.Success.CreateBody;
        }

        return new Confirmation
        {
            Title = title,
            Body = body
        };
    }

    private string GenerateErrorMessage(ManageDocumentsViewModel model)
    {
        string message = string.Empty;

        if (!string.IsNullOrEmpty(model.SelectedNewsId) && model.DocumentData.Id != null)
        {
            return Messages.NewsArticle.Errors.UpdatedError;
        }
        else if (model.DocumentData.Id != null && string.IsNullOrEmpty(model.SelectedNewsId))
        {
            return Messages.NewsArticle.Errors.UpdatedError;
        }
        else if (model.DocumentData.Id == null && string.IsNullOrEmpty(model.SelectedNewsId))
        {
            return Messages.NewsArticle.Errors.CreatedError;
        }

        return message;
    }

    private bool CheckValidUrlAction(string docAction)
    {
        if (!string.IsNullOrEmpty(docAction))
        {
            foreach (var e in Enum.GetValues(typeof(EditorActions)))
            {
                var fieldInfo = e.GetType().GetField(e.ToString());
                if (fieldInfo.Name.ToLower() == docAction.ToLower())
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckValidUrlDocType(string urlDocType)
    {
        if (!string.IsNullOrEmpty(urlDocType))
        {
            foreach (var e in Enum.GetValues(typeof(DocumentType)))
            {
                var fieldInfo = e.GetType().GetField(e.ToString());
                if (fieldInfo.Name.ToLower() == urlDocType.ToLower())
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Task<ViewResult> GenerateErrorView(string userErrorMessage)
    {
        var userErrorModel = new UserErrorViewModel()
        {
            UserErrorMessage = userErrorMessage,
            BackButton = new(true, "ManageDocuments", "ManageDocuments")
        };

        return Task.FromResult(View("../Admin/ManageDocuments/Error", userErrorModel));
    }
}
