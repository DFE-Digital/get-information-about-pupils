using System.Linq.Expressions;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification;
public sealed class PublishedArticleSpecification : ISpecification<NewsArticle>
{
    private readonly bool onlyPublished;

    public PublishedArticleSpecification(bool onlyPublished)
    {
        this.onlyPublished = onlyPublished;
    }

    public bool IncludeOnlyPublished => onlyPublished;

    public bool IsSatisfiedBy(NewsArticle article) => ToExpression().Compile().Invoke(article);
    public Expression<Func<NewsArticle, bool>> ToExpression() => (article) => !onlyPublished || article.Published;
}
