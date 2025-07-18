﻿using DfE.GIAP.Service.Download;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Core.Models.Glossary;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;

namespace DfE.GIAP.Web.Controllers;

public class GlossaryController : Controller
{
    private readonly IDownloadService _downloadService;
    private readonly IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> _getContentByPageKeyUseCase;

    public GlossaryController(
        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> getContentByPageKeyUseCase,
        IDownloadService downloadService)
    {
        ArgumentNullException.ThrowIfNull(getContentByPageKeyUseCase);
        ArgumentNullException.ThrowIfNull(downloadService);
        _getContentByPageKeyUseCase = getContentByPageKeyUseCase;
        _downloadService = downloadService;
    }

    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        const string GlossaryPageKey = "Glossary";

        GetContentByPageKeyUseCaseResponse response =
           await _getContentByPageKeyUseCase.HandleRequestAsync(
               new GetContentByPageKeyUseCaseRequest(pageKey: GlossaryPageKey));

        IEnumerable<MetaDataDownload> downloadList = await _downloadService.GetGlossaryMetaDataDownloadList().ConfigureAwait(false);

        GlossaryViewModel model = new()
        {
            Response = response.Content,
            MetaDataDownloadList = downloadList.OrderByDescending(x => x.Date).ToList()
        };

        return View(model);

    }

    [HttpGet]
    public async Task<FileStreamResult> GetBulkUploadTemplateFile(string name)
    {
        var ms = new MemoryStream();
        await _downloadService.GetGlossaryMetaDataDownFileAsync(name, ms, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));

        ms.Position = 0;

        return new FileStreamResult(ms, MediaTypeNames.Text.Plain)
        {
            FileDownloadName = name
        };
    }
}

