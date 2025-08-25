using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.PrePreparedDownload;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers.PreparedDownload;

[Route(Routes.PrePreparedDownloads.PreparedDownloadsController)]
public class PreparedDownloadsController : Controller
{
    private readonly IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse> _getPrePreparedFilesUseCase;
    private readonly IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse> _downloadPrePreparedFileUseCase;

    public PreparedDownloadsController(
         IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse> getPrePreparedFilesUseCase,
        IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse> downloadPrePreparedFileUseCase)
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

        GetPreparedFilesRequest request = new(pathContext);
        GetPreparedFilesResponse response = await _getPrePreparedFilesUseCase
            .HandleRequestAsync(request);

        PreparedDownloadsViewModel model = new()
        {
            PreparedDownloadFiles = response.BlobStorageItems
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

    [Route(Routes.PrePreparedDownloads.DownloadPrePreparedFileAction)]
    public async Task<FileStreamResult> DownloadPrePreparedFile(string name)
    {
        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(
            organisationScope: User.GetOrganisationScope(),
            uniqueIdentifier: User.GetUniqueIdentifier(),
            localAuthorityNumber: User.GetLocalAuthorityNumberForLocalAuthority(),
            uniqueReferenceNumber: User.GetUniqueReferenceNumber());

        DownloadPreparedFileRequest request = new(name, pathContext);
        DownloadPreparedFileResponse response = await _downloadPrePreparedFileUseCase
            .HandleRequestAsync(request);

        return new FileStreamResult(response.FileStream, response.ContentType)
        {
            FileDownloadName = response.FileName
        };
    }
}
