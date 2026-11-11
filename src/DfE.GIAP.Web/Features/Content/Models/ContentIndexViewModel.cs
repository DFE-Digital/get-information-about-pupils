using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Web.Features.Content.Models;

public class ContentIndexViewModel
{
    [Required(ErrorMessage = "Please select an option.")]
    public ContentManagementOption SelectedOption { get; set; }
    public List<ContentOptionViewModel> Options { get; set; }
}

public class ContentOptionViewModel
{
    public ContentManagementOption Value { get; set; }
    public string DisplayName { get; set; }
}
