namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
public sealed class FilterNewsArticleRequest
{
    private FilterNewsArticleRequest(IEnumerable<NewsArticleStateFilter> states)
    {
        // Validate the states
        ArgumentNullException.ThrowIfNull(states);
        if (!states.Any())
        {
            throw new ArgumentException("requested states is empty");
        }

        if (states.Contains(NewsArticleStateFilter.ArchivedOnly) && states.Contains(NewsArticleStateFilter.NotArchived))
        {
            throw new ArgumentException($"Conflict: Unable to request {nameof(NewsArticleStateFilter.ArchivedOnly)} and {nameof(NewsArticleStateFilter.NotArchived)}");
        }

        if (states.Contains(NewsArticleStateFilter.PublishedIncludeDrafts) && states.Contains(NewsArticleStateFilter.PublishedOnly))
        {
            throw new ArgumentException($"Conflict: Unable to request {nameof(NewsArticleStateFilter.PublishedIncludeDrafts)} and {nameof(NewsArticleStateFilter.PublishedOnly)}");
        }

        // Consistent default request states
        List<NewsArticleStateFilter> appendStates = [];

        if (!states.Contains(NewsArticleStateFilter.ArchivedOnly))
        {
            appendStates.Add(NewsArticleStateFilter.NotArchived);
        }

        States = states.Concat(appendStates)
            .Distinct()
            .ToArray()
            .AsReadOnly();
    }

    internal IReadOnlyCollection<NewsArticleStateFilter> States { get; }

    public static FilterNewsArticleRequest All() => new([NewsArticleStateFilter.PublishedIncludeDrafts]);
    public static FilterNewsArticleRequest Archived() => new([NewsArticleStateFilter.ArchivedOnly]);
    public static FilterNewsArticleRequest Published() => new([NewsArticleStateFilter.PublishedOnly]);
}
