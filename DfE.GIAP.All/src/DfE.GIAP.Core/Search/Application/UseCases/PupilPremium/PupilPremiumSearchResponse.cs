using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
public record PupilPremiumSearchResponse
{
    public PupilPremiumSearchResponse(SearchResponseStatus status, int? totalNumberOfResults = null)
    {
        Status = status;

        TotalNumberOfResults = new(totalNumberOfResults);
    }

    /// <summary>
    /// Gets the collection of learner search results returned by the query.
    /// </summary>
    public PupilPremiumLearners? LearnerSearchResults { get; init; }

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
