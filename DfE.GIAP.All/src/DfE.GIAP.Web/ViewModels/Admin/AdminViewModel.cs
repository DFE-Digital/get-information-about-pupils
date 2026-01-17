using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Admin;

[ExcludeFromCodeCoverage]
public class AdminViewModel
{
    public bool IsAdmin { get; set; }
    public string SelectedAdminOption { get; set; }
}
