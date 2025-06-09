using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download.SecurityReport;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.Admin.SecurityReports;

[Route(Routes.Application.Admin)]
[Authorize(Policy = Policy.RequireAdminApproverAccess)]
public class SecurityReportByPupilStudentRecordController : Controller
{
    private readonly IDownloadSecurityReportByUpnUlnService _downloadSecurityReportByUpnUlnService;

    public SecurityReportByPupilStudentRecordController(IDownloadSecurityReportByUpnUlnService downloadSecurityReportByUpnUlnService)
    {
        _downloadSecurityReportByUpnUlnService = downloadSecurityReportByUpnUlnService ??
            throw new ArgumentNullException(nameof(downloadSecurityReportByUpnUlnService));
    }



    [HttpGet]
    [Route(Routes.SecurityReports.SecurityReportsByUpnUln)]
    public IActionResult SecurityReportsByUpnUln()
    {
        var model = new SecurityReportsByUpnUlnViewModel();
        return View("../Admin/SecurityReports/SecurityReportsByUpnUln", model);
    }
    [HttpPost]
    [Route(Routes.SecurityReports.SecurityReportsByUpnUln)]
    public async Task<IActionResult> DownloadSecurityReportByUpnUln(SecurityReportsByUpnUlnViewModel model)
    {
        if (ModelState.IsValid)
        {
            model.DownloadFileType = DownloadFileType.CSV;
            var downloadFile = model.UPNSearch ?
                await _downloadSecurityReportByUpnUlnService.GetSecurityReportByUpn(model.UpnUln, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false) :
                await _downloadSecurityReportByUpnUlnService.GetSecurityReportByUln(model.UpnUln, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

            if (downloadFile?.Bytes != null)
            {
                return SearchDownloadHelper.DownloadFile(downloadFile);
            }
            else
            {
                model.ErrorDetails = DownloadErrorMessages.NoDataForDownload;
            }
        }

        return View("../Admin/SecurityReports/SecurityReportsByUpnUln", model);
    }
}
