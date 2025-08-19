using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PreparedDownloads.Application.Enums;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers;

public class GlossaryController : Controller
{
    private readonly IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse> _getPrePreparedFilesUseCase;
    private readonly IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse> _downloadPrePreparedFileUseCase;

    public GlossaryController(
        IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse> getPrePreparedFilesUseCase,
        IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse> downloadPrePreparedFileUseCase)
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

        GetPreparedFilesRequest request = new(pathContext);
        GetPreparedFilesResponse response = await _getPrePreparedFilesUseCase
            .HandleRequestAsync(request);

        GlossaryViewModel model = new()
        {
            PreparedMetadataFiles = response.BlobStorageItems
            .Select(item => new PreparedFileViewModel
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

        DownloadPreparedFileRequest request = new(name, pathContext);
        DownloadPreparedFileResponse response = await _downloadPrePreparedFileUseCase
            .HandleRequestAsync(request);

        return new FileStreamResult(response.FileStream, response.ContentType)
        {
            FileDownloadName = response.FileName
        };
    }
}

