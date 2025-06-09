using DfE.GIAP.Core.Common.Application.Specification;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification.Factory;
internal interface IFilterNewsArticleSpecificationFactory
{
    ISpecification<NewsArticle> Create(NewsArticleStateFilter filter);
}
