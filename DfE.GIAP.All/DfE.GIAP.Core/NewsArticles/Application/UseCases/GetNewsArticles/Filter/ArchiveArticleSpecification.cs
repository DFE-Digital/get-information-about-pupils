using System.Linq.Expressions;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles.Filter;
public sealed class ArchivedArticleSpecification : IFilterSpecification<NewsArticle>
{
    private readonly bool _isArchived;

    public ArchivedArticleSpecification(bool isArchived)
    {
        _isArchived = isArchived;
    }

    public bool IsSatisfiedBy(NewsArticle article) => ToExpression().Compile().Invoke(article);
    public Expression<Func<NewsArticle, bool>> ToExpression() => (article) => article.Archived == _isArchived;
    public string ToFilterQuery(string alias = "c") => $"{alias}.Archived = {AsFilterValue(_isArchived)}";
    private static string AsFilterValue(bool value) => value ? "true" : "false";
}
