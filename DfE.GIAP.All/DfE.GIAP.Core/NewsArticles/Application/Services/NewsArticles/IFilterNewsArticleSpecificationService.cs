using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles;
public interface IFilterNewsArticleSpecificationService
{
    ISpecification<NewsArticle> Create(IEnumerable<NewsArticleStateFilter> state);
}
