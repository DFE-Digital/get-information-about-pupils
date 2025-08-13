using Microsoft.AspNetCore.Mvc.Rendering;

namespace DfE.GIAP.Web.ViewModels.Admin.ManageNewsArticles;

public class ManageNewsArticlesViewModel
{
    public string SelectedNewsId { get; set; }
    public SelectList NewsArticleList { get; set; }
    public BackButtonViewModel BackButton { get; set; }
}
