using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Validation;
using DfE.GIAP.Web.ViewModels.Admin.SecurityReports;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Admin;

[ExcludeFromCodeCoverage]
public class SecurityReportsByUpnUlnViewModel : BaseSecurityReportsViewModel
{
    [SecurityReportsUpnUlnValidation]
    public string UpnUln { get; set; }
    public DownloadFileType DownloadFileType { get; set; }

    public bool UPNSearch => UpnUln?.Length == 13;

    public bool ULNSearch => UpnUln?.Length == 10;

    public BackButtonViewModel BackButton { get; set; }
}
