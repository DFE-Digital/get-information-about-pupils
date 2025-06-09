using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles.Specification;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles;
internal sealed class FilterNewsArticleSpecificationService : IFilterNewsArticleSpecificationService
{

    private static readonly Dictionary<NewsArticleStateFilter, Func<ISpecification<NewsArticle>>> _map =
        new()
        {
             { NewsArticleStateFilter.PublishedIncludeDrafts, () => new PublishedArticleSpecification(false) },
             { NewsArticleStateFilter.PublishedOnly, () => new PublishedArticleSpecification(true) },
             { NewsArticleStateFilter.NotArchived, () => new ArchivedArticleSpecification(false) },
             { NewsArticleStateFilter.ArchivedOnly, () => new ArchivedArticleSpecification(true) }
        };

    public ISpecification<NewsArticle> Create(IEnumerable<NewsArticleStateFilter> state)
    {
        List<ISpecification<NewsArticle>> specificationFilters = [];

        state.ToList().ForEach((currentState) =>
        {
            if (!_map.TryGetValue(currentState, out Func<ISpecification<NewsArticle>>? specificationHandler))
            {
                throw new ArgumentException($"unable to find state {currentState}");
            }
            ISpecification<NewsArticle> spec = specificationHandler.Invoke();
            specificationFilters.Add(spec);
        });

        return specificationFilters.Count == 1 ?
            specificationFilters.Single() :
                specificationFilters.Aggregate((a, b) => new AndSpecificaton<NewsArticle>(a, b));
    }
}
