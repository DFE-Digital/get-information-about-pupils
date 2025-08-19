using System.Net.Mime;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Models.Glossary;
using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DfE.GIAP.Web.Controllers;

public class GlossaryController : Controller
{
    private readonly IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse> _downloadPrePreparedFileUseCase;
    private readonly IDownloadService _downloadService;

    public GlossaryController(
        IDownloadService downloadService,
        IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse> downloadPrePreparedFileUseCase)
    {
        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;

        ArgumentNullException.ThrowIfNull(downloadPrePreparedFileUseCase);
        _downloadPrePreparedFileUseCase = downloadPrePreparedFileUseCase;
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
            MetaDataDownloadList = downloadList
                .OrderByDescending(x => x.Date)
                .ToList()
        };

        return View(model);

    }

    [HttpGet]
    public async Task<FileStreamResult> GetBulkUploadTemplateFile(string name)
    {
        BlobStoragePathContext pathContext = BlobStoragePathContext
            .Create(OrganisationScope.AllUsers);

        DownloadPrePreparedFileRequest request = new(name, pathContext);
        DownloadPrePreparedFileResponse response = await _downloadPrePreparedFileUseCase
            .HandleRequestAsync(request);

        return new FileStreamResult(response.FileStream, response.ContentType)
        {
            FileDownloadName = response.FileName
        };
    }
}

