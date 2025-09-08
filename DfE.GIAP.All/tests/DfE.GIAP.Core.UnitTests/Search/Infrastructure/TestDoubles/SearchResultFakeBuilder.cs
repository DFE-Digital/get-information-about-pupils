using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// A fluent builder for creating fake <see cref="SearchResult{T}"/> collections
/// containing <see cref="Learner"/> documents for use in unit tests.
/// </summary>
/// <remarks>
/// This builder allows you to:
/// <list type="bullet">
/// <item>Generate an empty search result set</item>
/// <item>Generate a randomised set of learner search results</item>
/// <item>Include a null document in the results (for edge case testing)</item>
/// </list>
/// </remarks>
internal class SearchResultFakeBuilder
{
    /// <summary>
    /// Backing store for the search results being built.
    /// </summary>
    private List<SearchResult<LearnerDataTransferObject>>? _searchResults;

    /// <summary>
    /// Configures the builder to produce an empty search result set.
    /// </summary>
    /// <returns>The current builder instance for fluent chaining.</returns>
    public SearchResultFakeBuilder WithEmptySearchResult()
    {
        _searchResults = [];
        return this;
    }

    /// <summary>
    /// Configures the builder to produce a randomised set of learner search results.
    /// </summary>
    /// <remarks>
    /// The number of results will be between 1 and 10, and each result will contain
    /// a randomly generated <see cref="Learner"/> document from <see cref="LearnerTestDouble"/>.
    /// </remarks>
    /// <returns>The current builder instance for fluent chaining.</returns>
    public SearchResultFakeBuilder WithSearchResults()
    {
        int amount = new Bogus.Faker().Random.Number(1, 10);
        List<SearchResult<LearnerDataTransferObject>> searchResults = new(capacity: amount);

        for (int i = 0; i < amount; i++)
        {
            searchResults
                .Add(SearchResultWithDocument(
                    LearnerDataTransferObjectTestDouble.Create()));
        }

        _searchResults = searchResults;
        return this;
    }

    /// <summary>
    /// Adds a search result with a null document to the current result set.
    /// Useful for testing null-handling logic in consuming code.
    /// </summary>
    /// <returns>The current builder instance for fluent chaining.</returns>
    public SearchResultFakeBuilder IncludeNullDocument()
    {
        _searchResults ??= [];
        _searchResults.Add(
            SearchModelFactory.SearchResult<LearnerDataTransferObject>(
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
    public static SearchResult<LearnerDataTransferObject> SearchResultWithDocument(
        LearnerDataTransferObject? document) =>
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
    public List<SearchResult<LearnerDataTransferObject>> Create() =>
        _searchResults ??
        throw new NullReferenceException(
            "No search results have been configured.");
}
