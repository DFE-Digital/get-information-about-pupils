using DfE.GIAP.Service.Download;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Core.Models.Glossary;

namespace DfE.GIAP.Web.Controllers;

public class GlossaryController : Controller
{
    private readonly IDownloadService _downloadService;

    public GlossaryController(IDownloadService downloadService)
    {
        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;
    }

    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        IEnumerable<MetaDataDownload> downloadList = await _downloadService
            .GetGlossaryMetaDataDownloadList()
            .ConfigureAwait(false);

        GlossaryViewModel model = new()
        {
            MetaDataDownloadList = downloadList.OrderByDescending(x => x.Date).ToList()
        };

        return View(model);

    }

    [HttpGet]
    public async Task<FileStreamResult> GetBulkUploadTemplateFile(string name)
    {
        MemoryStream ms = new();
        await _downloadService
            .GetGlossaryMetaDataDownFileAsync(name, ms, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));

        ms.Position = 0;
        return new FileStreamResult(ms, MediaTypeNames.Text.Plain)
        {
            FileDownloadName = name
        };
    }
}

