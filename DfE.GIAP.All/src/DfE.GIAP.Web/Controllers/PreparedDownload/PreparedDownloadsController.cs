using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;
using DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.GetPrePreparedFiles;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.PrePreparedDownload;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers.PreparedDownload;

[Route(Routes.PrePreparedDownloads.PreparedDownloadsController)]
public class PreparedDownloadsController : Controller
{
    private readonly IUseCase<GetPrePreparedFilesRequest, GetPrePreparedFilesResponse> _getPrePreparedFilesUseCase;
    private readonly IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse> _downloadPrePreparedFileUseCase;

    public PreparedDownloadsController(
         IUseCase<GetPrePreparedFilesRequest, GetPrePreparedFilesResponse> getPrePreparedFilesUseCase,
        IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse> downloadPrePreparedFileUseCase)
    {
        ArgumentNullException.ThrowIfNull(getPrePreparedFilesUseCase);
        _getPrePreparedFilesUseCase = getPrePreparedFilesUseCase;

        ArgumentNullException.ThrowIfNull(downloadPrePreparedFileUseCase);
        _downloadPrePreparedFileUseCase = downloadPrePreparedFileUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(
            organisationScope: User.GetOrganisationScope(),
            uniqueIdentifier: User.GetUniqueIdentifier(),
            localAuthorityNumber: User.GetLocalAuthorityNumberForLocalAuthority(),
            uniqueReferenceNumber: User.GetUniqueReferenceNumber());

        GetPrePreparedFilesRequest request = new(pathContext);
        GetPrePreparedFilesResponse response = await _getPrePreparedFilesUseCase
            .HandleRequestAsync(request);

        PrePreparedDownloadsViewModel model = new()
        {
            PrePreparedDownloadList = response.BlobStorageItems
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

    [Route(Routes.PrePreparedDownloads.DownloadPrePreparedFileAction)]
    public async Task<FileStreamResult> DownloadPrePreparedFile(string name)
    {
        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(
            organisationScope: User.GetOrganisationScope(),
            uniqueIdentifier: User.GetUniqueIdentifier(),
            localAuthorityNumber: User.GetLocalAuthorityNumberForLocalAuthority(),
            uniqueReferenceNumber: User.GetUniqueReferenceNumber());

        DownloadPrePreparedFileRequest request = new(name, pathContext);
        DownloadPrePreparedFileResponse response = await _downloadPrePreparedFileUseCase
            .HandleRequestAsync(request);

        return new FileStreamResult(response.FileStream, response.ContentType)
        {
            FileDownloadName = response.FileName
        };
    }
}
