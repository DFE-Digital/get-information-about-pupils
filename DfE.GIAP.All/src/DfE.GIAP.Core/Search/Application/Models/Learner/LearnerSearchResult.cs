using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Application.Models.Learner;

/// <summary>
/// Represents the outcome of a further education learner search operation.
/// Contains meta-data, result payload, and any associated facets.
/// </summary>
public sealed class LearnerSearchResult
{
    /// <summary>
    /// Initializes a new instance with the specified search response status.
    /// </summary>
    /// <param name="status">The status code or enum representing search outcome.</param>
    public LearnerSearchResult(SearchResponseStatus status)
    {
        Status = status;
    }

    /// <summary>
    /// Gets the returned learner results, if any were found.
    /// </summary>
    public Learners? LearnerResults { get; init; }

    /// <summary>
    /// Gets the facet groupings returned from the search, used for aggregation or filtering context.
    /// </summary>
    public SearchFacets? FacetedResults { get; init; }

    /// <summary>
    /// Gets the status of the search, indicating success, failure, or other condition.
    /// </summary>
    public SearchResponseStatus Status { get; }

    /// <summary>
    /// Gets the total number of distinct learners involved in the leaner result set.
    /// This may be used for summary metrics or analytics.
    /// </summary>
    public int TotalNumberOfLearners { get; init; }
}
