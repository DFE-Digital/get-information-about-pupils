using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles.FilterFactory.Specification;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles.Factory;
internal sealed class FilterNewsArticleSpecificationFactory : IFilterNewsArticleSpecificationFactory
{
    public IFilterSpecification<NewsArticle> Create(IEnumerable<NewsArticleStateFilter> state)
    {
        List<IFilterSpecification<NewsArticle>> specificationFilters = [];

        state.ToList().ForEach(newsArticleFilterState =>
        {
            specificationFilters.Add(
                newsArticleFilterState switch
                {
                    NewsArticleStateFilter.PublishedIncludeDrafts => new PublishedArticleSpecification(includeOnlyPublished: false),
                    NewsArticleStateFilter.PublishedOnly => new PublishedArticleSpecification(includeOnlyPublished: true),
                    NewsArticleStateFilter.NotArchived => new ArchivedArticleSpecification(isArchived: false),
                    NewsArticleStateFilter.ArchivedOnly => new ArchivedArticleSpecification(isArchived: true),
                    _ => throw new NotImplementedException()
                });
        });

        return specificationFilters.Count == 1 ?
            specificationFilters.Single() :
                specificationFilters.Aggregate((a, b) => new AndSpecificaton<NewsArticle>(a, b));
    }
}
