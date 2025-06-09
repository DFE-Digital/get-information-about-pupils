using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification;
public interface INewsArticleSpecificationService
{
    ISpecification<NewsArticle> CreateSpecification(IEnumerable<NewsArticleStateFilter> states);
}
