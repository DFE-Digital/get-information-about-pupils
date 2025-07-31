using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Admin;

[ExcludeFromCodeCoverage]
public class AdminViewModel
{
    public bool IsAdmin { get; set; }
    public bool IsApprover { get; set; }
    public bool IsDepartmentUser { get; set; }
    public bool IsOrganisationEstablishment { get; set; }
    public bool IsOrganisationEstablishmentWithFurtherEducation { get; set; }
    public string SelectedAdminOption { get; set; }
    public string SelectedOrganisationOption { get; set; }
    public BackButtonViewModel BackButton { get; set; }
}
