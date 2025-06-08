using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles.Factory;
public interface IFilterNewsArticleSpecificationFactory
{
    IFilterSpecification<NewsArticle> Create(IEnumerable<NewsArticleStateFilter> state);
}
