using System.Linq.Expressions;
using DfE.GIAP.Core.Common.Application.Specification;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification.Specifications;
public sealed class PublishedArticleSpecification : ISpecification<NewsArticle>
{
    public PublishedArticleSpecification(bool onlyPublished)
    {
        OnlyPublished = onlyPublished;
    }

    public bool OnlyPublished { get; }

    public bool IsSatisfiedBy(NewsArticle article) => ToExpression().Compile().Invoke(article);
    public Expression<Func<NewsArticle, bool>> ToExpression() => (article) => !OnlyPublished || article.Published;
}
