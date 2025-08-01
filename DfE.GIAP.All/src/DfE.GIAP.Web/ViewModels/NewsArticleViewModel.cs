using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Web.Constants;

namespace DfE.GIAP.Web.ViewModels.Admin.ManageNewsArticles;

public class NewsArticleViewModel
{
    public string Id { get; set; }

    [Required(ErrorMessage = Messages.Common.Errors.TitleRequired)]
    [MaxLength(64, ErrorMessage = Messages.Common.Errors.TitleLength)]
    public string Title { get; set; }

    [Required(ErrorMessage = Messages.Common.Errors.BodyRequired)]
    public string Body { get; set; }

    public bool Pinned { get; set; }
    public bool Published { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
