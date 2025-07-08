using DfE.GIAP.Web.ViewModels.Admin.SecurityReports;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Admin;

[ExcludeFromCodeCoverage]
public class SecurityReportsForYourOrganisationViewModel : BaseSecurityReportsViewModel
{
    public string DocumentId { get; set; }
    public bool HasInvalidDocumentList { get; set; }
    public List<SelectListItem> SecurityReportTypes { get; set; }
    public BackButtonViewModel BackButton { get; set; }
}
