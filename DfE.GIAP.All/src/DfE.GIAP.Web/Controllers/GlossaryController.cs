using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;
using DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.GetPrePreparedFiles;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class GlossaryController : Controller
{
    private readonly IUseCase<GetPrePreparedFilesRequest, GetPrePreparedFilesResponse> _getPrePreparedFilesUseCase;
    private readonly IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse> _downloadPrePreparedFileUseCase;

    public GlossaryController(
        IUseCase<GetPrePreparedFilesRequest, GetPrePreparedFilesResponse> getPrePreparedFilesUseCase,
        IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse> downloadPrePreparedFileUseCase)
    {
        ArgumentNullException.ThrowIfNull(getPrePreparedFilesUseCase);
        _getPrePreparedFilesUseCase = getPrePreparedFilesUseCase;

        ArgumentNullException.ThrowIfNull(downloadPrePreparedFileUseCase);
        _downloadPrePreparedFileUseCase = downloadPrePreparedFileUseCase;
    }

    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        BlobStoragePathContext pathContext = BlobStoragePathContext
             .Create(OrganisationScope.AllUsers);

        GetPrePreparedFilesRequest request = new(pathContext);
        GetPrePreparedFilesResponse response = await _getPrePreparedFilesUseCase
            .HandleRequestAsync(request);

        GlossaryViewModel model = new()
        {
            PrePreparedMetadataFiles = response.BlobStorageItems
            .Select(item => new PrePreparedFileViewModel
            {
                Name = item.Name,
                Date = item.LastModified?.UtcDateTime ?? DateTime.MinValue
            })
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

