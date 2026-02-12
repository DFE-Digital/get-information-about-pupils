using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.Features.Search.LegacyModels;

[ExcludeFromCodeCoverage]
public class RedirectUrls
{
    public string SurnameFilterURL { get; set; }
    public string FormAction { get; set; }
    public string DobFilterUrl { get; set; }
    public string ForenameFilterUrl { get; set; }
    public string MiddlenameFilterUrl { get; set; }
    public string SexFilterUrl { get; set; }
    public string RemoveAction { get; set; }
}
