using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Helper;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Middleware;
using System;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Glossary;
using System.Collections.Generic;

namespace DfE.GIAP.Web.Controllers;

public class GlossaryController : Controller
{
    private readonly IContentService _contentService;
    private readonly IDownloadService _downloadService;

    public GlossaryController(IContentService contentService, IDownloadService downloadService)
    {
        _contentService = contentService ??
            throw new ArgumentNullException(nameof(contentService));
        _downloadService = downloadService ??
            throw new ArgumentNullException(nameof(downloadService));
    }

    [AllowWithoutConsent]
    public async Task<IActionResult> Index()
    {
        CommonResponseBody glossaryResults = await _contentService.GetContent(DocumentType.Glossary).ConfigureAwait(false);
        List<MetaDataDownload> downloadList = await _downloadService.GetGlossaryMetaDataDownloadList().ConfigureAwait(false);

        var model = new GlossaryViewModel
        {
            Response = glossaryResults.ConvertToViewModel(),
            MetaDataDownloadList = downloadList.OrderByDescending(x => x.Date).ToList()
        };

        return View(model);

    }

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

