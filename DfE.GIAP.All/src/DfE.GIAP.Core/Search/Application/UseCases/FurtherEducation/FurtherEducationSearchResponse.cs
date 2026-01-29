using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation;

/// <summary>
/// Represents the result of a search by pupil name in further education.
/// Includes matched pupils, faceted data, result meta-data, and status.
/// </summary>
public sealed class FurtherEducationSearchResponse
{
    /// <summary>
    /// Initializes a new response with the specified search status and total result count.
    /// </summary>
    /// <param name="status">Indicates the outcome of the search operation.</param>
    /// <param name="totalNumberOfResults">
    /// The total number of matching learner records found. Defaults to zero if null or negative.
    /// </param>
    public FurtherEducationSearchResponse(SearchResponseStatus status, int? totalNumberOfResults = null)
    {
        Status = status;
        TotalNumberOfResults = new SearchResultCount(totalNumberOfResults);
    }

    /// <summary>
    /// Gets the collection of learner search results returned by the query.
    /// </summary>
    public FurtherEducationLearners? LearnerSearchResults { get; init; }

    /// <summary>
    /// Gets the faceted aggregation results used for UI filtering, analytics, or navigation.
    /// </summary>
    public SearchFacets? FacetedResults { get; init; }

    /// <summary>
    /// Gets the overall status of the search execution.
    /// </summary>
    public SearchResponseStatus Status { get; }

    /// <summary>
    /// Gets the total number of learner's matched by the search criteria.
    /// Guaranteed to be non-negative.
    /// </summary>
    public SearchResultCount TotalNumberOfResults { get; }
}
