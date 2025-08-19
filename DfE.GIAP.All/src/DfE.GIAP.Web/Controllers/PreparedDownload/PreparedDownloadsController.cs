using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.PreparedDownloads;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.ViewModels.PrePreparedDownload;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Controllers.PreparedDownload;

[Route(Routes.PrePreparedDownloads.PreparedDownloadsController)]
public class PreparedDownloadsController : Controller
{
    private readonly IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse> _downloadPrePreparedFileUseCase;
    private readonly IPrePreparedDownloadsService _prePreparedDownloadsService;

    public PreparedDownloadsController(
        IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse> downloadPrePreparedFileUseCase,
        ICommonService commonService,
        IPrePreparedDownloadsService prePreparedDownloadsService)
    {
        ArgumentNullException.ThrowIfNull(downloadPrePreparedFileUseCase);
        _downloadPrePreparedFileUseCase = downloadPrePreparedFileUseCase;

        _prePreparedDownloadsService = prePreparedDownloadsService ??
            throw new ArgumentNullException(nameof(prePreparedDownloadsService));

    }
    [HttpGet]
    public async Task<IActionResult> GetPreparedDownloadsAsync()
    {
        var downloadList = await _prePreparedDownloadsService.GetPrePreparedDownloadsList(AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
                                                                                          User.IsOrganisationLocalAuthority(),
                                                                                          User.IsOrganisationMultiAcademyTrust(),
                                                                                          User.IsOrganisationEstablishment(),
                                                                                          User.IsOrganisationSingleAcademyTrust(),
                                                                                          User.GetEstablishmentNumber(),
                                                                                          User.GetUniqueIdentifier(),
                                                                                          User.GetLocalAuthorityNumberForLocalAuthority(),
                                                                                          User.GetUniqueReferenceNumber())
                                                                                          .ConfigureAwait(false);

        var model = new PrePreparedDownloadsViewModel
        {
            PrePreparedDownloadList = downloadList.OrderByDescending(x => x.Date).ToList()
        };

        return View("~/Views/PrePreparedDownloads/PrePreparedDownload.cshtml", model);
    }

    [Route(Routes.PrePreparedDownloads.DownloadPrePreparedFileAction)]
    public async Task<FileStreamResult> DownloadPrePreparedFile(string name, DateTime fileUploadedDate)
    {
        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(
            organisationScope: User.GetOrganisationType(),
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
