using Azure.Search.Documents.Models;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

internal class SearchResultFakeBuilder<T> where T : class
{
    private List<SearchResult<T>>? _searchResults;

    public SearchResultFakeBuilder<T> WithEmptySearchResult()
    {
        _searchResults = [];
        return this;
    }

    public SearchResultFakeBuilder<T> WithSearchResults(T result)
    {
        int amount = new Bogus.Faker().Random.Number(1, 10);
        List<SearchResult<T>> searchResults = new(capacity: amount);

        for (int i = 0; i < amount; i++)
        {
            searchResults
                .Add(SearchResultWithDocument(result));
        }

        _searchResults = searchResults;
        return this;
    }

    /// <summary>
    /// Adds a search result with a null document to the current result set.
    /// Useful for testing null-handling logic in consuming code.
    /// </summary>
    /// <returns>The current builder instance for fluent chaining.</returns>
    public SearchResultFakeBuilder<T> IncludeNullDocument()
    {
        _searchResults ??= [];
        _searchResults.Add(
            SearchModelFactory.SearchResult<T>(
                document: null!,
                score: 1.00,
                highlights: new Dictionary<string, IList<string>>()
            ));

        return this;
    }

    /// <summary>
    /// Creates a <see cref="SearchResult{T}"/> containing the specified learner document.
    /// </summary>
    /// <param name="document">The learner document to include in the search result.</param>
    /// <returns>A <see cref="SearchResult{T}"/> wrapping the provided document.</returns>
    public static SearchResult<T> SearchResultWithDocument(
        T? document) =>
        SearchModelFactory.SearchResult(
            document!,
            score: 1.00,
            highlights: new Dictionary<string, IList<string>>()
        );

    /// <summary>
    /// Finalises and returns the configured list of search results.
    /// </summary>
    /// <returns>The configured list of <see cref="SearchResult{T}"/> objects.</returns>
    /// <exception cref="NullReferenceException">
    /// Thrown if no search results have been configured before calling this method.
    /// </exception>
    public List<SearchResult<T>> Create() =>
        _searchResults ??
        throw new NullReferenceException(
            "No search results have been configured.");
}
