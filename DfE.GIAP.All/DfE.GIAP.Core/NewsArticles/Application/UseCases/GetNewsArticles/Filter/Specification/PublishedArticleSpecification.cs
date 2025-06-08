using System.Linq.Expressions;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles.FilterFactory.Specification;
public sealed class PublishedArticleSpecification : IFilterSpecification<NewsArticle>
{
    private readonly bool _includeOnlyPublished;

    public PublishedArticleSpecification(bool includeOnlyPublished)
    {
        _includeOnlyPublished = includeOnlyPublished;
    }

    public bool IsSatisfiedBy(NewsArticle article) => ToExpression().Compile().Invoke(article);
    public Expression<Func<NewsArticle, bool>> ToExpression() => (article) => !_includeOnlyPublished || article.Published;
    public string ToFilterQuery(string alias = "c") => _includeOnlyPublished ? $"{alias}.Published = true" : string.Empty;
}
