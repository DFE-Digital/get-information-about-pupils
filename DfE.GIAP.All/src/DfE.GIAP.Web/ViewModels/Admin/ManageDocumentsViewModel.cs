using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Web.Constants;

namespace DfE.GIAP.Web.ViewModels.Admin;

[ExcludeFromCodeCoverage]
public class ManageDocumentsViewModel
{
    public Document DocumentList { get; set; }
    public CommonResponseBodyViewModel DocumentData { get; set; }
    public string SelectedNewsId { get; set; }
    public bool HasInvalidNewsList { get; set; }
    public Confirmation Confirmation { get; set; }
    public NewsArticleViewModel NewsArticle { get; set; }
    public BackButtonViewModel BackButton { get; set; }
}

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
