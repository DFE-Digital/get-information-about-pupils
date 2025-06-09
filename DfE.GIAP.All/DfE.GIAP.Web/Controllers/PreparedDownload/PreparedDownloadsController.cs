using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.LoggingEvent;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.PreparedDownloads;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.DSIUser;
using DfE.GIAP.Web.ViewModels.PrePreparedDownload;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.PreparedDownload;

[Route(Routes.PrePreparedDownloads.PreparedDownloadsController)]
public class PreparedDownloadsController : Controller
{
    private readonly IPrePreparedDownloadsService _prePreparedDownloadsService;
    private readonly ICommonService _commonService;

    public PreparedDownloadsController(ICommonService commonService, IPrePreparedDownloadsService prePreparedDownloadsService)
    {
        _commonService = commonService ??
            throw new ArgumentNullException(nameof(commonService));
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

        var loggingEvent = new LoggingEvent
        {
            UserGuid = User.GetUserId(),
            UserEmail = User.GetUserEmail(),
            UserGivenName = User.GetUserGivenName(),
            UserSurname = User.GetUserSurname(),
            OrganisationGuid = User.GetOrganisationId(),
            OrganisationName = User.GetOrganisationName(),
            EstablishmentNumber = User.GetEstablishmentNumber(),
            LocalAuthorityNumber = User.GetLocalAuthorityNumberForEstablishment(),
            OrganisationCategoryID = User.GetOrganisationCategoryID(),
            UKProviderReferenceNumber = User.GetUKProviderReferenceNumber(),
            UniqueIdentifier = User.GetUniqueIdentifier(),
            UniqueReferenceNumber = User.GetUniqueReferenceNumber(),
            OrganisationType = DSIUserHelper.GetOrganisationType(User.GetOrganisationCategoryID()),
            GIAPUserRole = DSIUserHelper.GetGIAPUserRole(User.IsAdmin(),
                                                         User.IsApprover(),
                                                         User.IsNormal()),
            SessionId = User.GetSessionId()
        };

        loggingEvent.ActionName = LogEventActionType.DownloadPrePreparedFile.ToString();
        loggingEvent.ActionDescription = LogEventActionType.DownloadPrePreparedFile.LogEventActionDescription();
        loggingEvent.PrePreparedDownloadsFileName = name;
        loggingEvent.PrePreparedDownloadsFileUploadedDate = DateTimeHelper.ConvertDateTimeToString(fileUploadedDate);
        //Logging Event
        _ = await _commonService.CreateLoggingEvent(loggingEvent);

        var ms = new MemoryStream();
        await _prePreparedDownloadsService.PrePreparedDownloadsFileAsync(name,
                                                                         ms,
                                                                         AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
                                                                         User.IsOrganisationLocalAuthority(),
                                                                         User.IsOrganisationMultiAcademyTrust(),
                                                                         User.IsOrganisationEstablishment(),
                                                                         User.IsOrganisationSingleAcademyTrust(),
                                                                         User.GetEstablishmentNumber(),
                                                                         User.GetUniqueIdentifier(),
                                                                         User.GetLocalAuthorityNumberForLocalAuthority(),
                                                                         User.GetUniqueReferenceNumber());

        ms.Position = 0;

        return new FileStreamResult(ms, MediaTypeNames.Text.Plain)
        {
            FileDownloadName = name
        };
    }
}
