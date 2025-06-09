using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles;
internal sealed class FilterNewsArticleSpecificationService : IFilterNewsArticleSpecificationService
{

    private static readonly Dictionary<NewsArticleStateFilter, Func<IFilterSpecification<NewsArticle>>> _map =
        new()
        {
             { NewsArticleStateFilter.PublishedIncludeDrafts, () => new PublishedArticleSpecification(false) },
             { NewsArticleStateFilter.PublishedOnly, () => new PublishedArticleSpecification(true) },
             { NewsArticleStateFilter.NotArchived, () => new ArchivedArticleSpecification(false) },
             { NewsArticleStateFilter.ArchivedOnly, () => new ArchivedArticleSpecification(true) }
        };

    public IFilterSpecification<NewsArticle> Create(IEnumerable<NewsArticleStateFilter> state)
    {
        List<IFilterSpecification<NewsArticle>> specificationFilters = [];

        state.ToList().ForEach((currentState) =>
        {
            if (!_map.TryGetValue(currentState, out Func<IFilterSpecification<NewsArticle>>? specificationHandler))
            {
                throw new ArgumentException($"unable to find state {currentState}");
            }
            IFilterSpecification<NewsArticle> spec = specificationHandler.Invoke();
            specificationFilters.Add(spec);
        });

        return specificationFilters.Count == 1 ?
            specificationFilters.Single() :
                specificationFilters.Aggregate((a, b) => new AndSpecificaton<NewsArticle>(a, b));
    }
}
