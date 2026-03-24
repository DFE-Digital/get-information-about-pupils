using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class NewsViewModel
{
    public IEnumerable<NewsArticleViewModel> NewsArticles { get; set; }
}
