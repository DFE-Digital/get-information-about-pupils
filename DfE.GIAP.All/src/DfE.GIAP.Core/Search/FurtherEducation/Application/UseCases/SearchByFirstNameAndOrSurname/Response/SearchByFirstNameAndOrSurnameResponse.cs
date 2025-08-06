using DfE.GIAP.Core.Search.Common.Application.Models;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Model;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;

namespace DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Response;

/// <summary>
/// Represents the result of a search by pupil name in further education.
/// Includes matched pupils, faceted data, result meta-data, and status.
/// </summary>
public sealed class SearchByFirstNameAndOrSurnameResponse
{
    /// <summary>
    /// Initializes a new response with the specified search status and total result count.
    /// </summary>
    /// <param name="status">Indicates the outcome of the search operation.</param>
    /// <param name="totalNumberOfResults">
    /// The total number of matching pupil records found. Defaults to zero if null or negative.
    /// </param>
    public SearchByFirstNameAndOrSurnameResponse(SearchResponseStatus status, int? totalNumberOfResults = null)
    {
        Status = status;
        TotalNumberOfResults = totalNumberOfResults is null || totalNumberOfResults < 0
            ? 0
            : totalNumberOfResults.Value;
    }

    /// <summary>
    /// Gets the collection of pupil search results returned by the query.
    /// </summary>
    public FurtherEducationPupils? PupilSearchResults { get; init; }

    /// <summary>
    /// Gets the faceted aggregation results used for UI filtering, analytics, or navigation.
    /// </summary>
    public FurtherEducationFacets? FacetedResults { get; init; }

    /// <summary>
    /// Gets the overall status of the search execution.
    /// </summary>
    public SearchResponseStatus Status { get; }

    /// <summary>
    /// Gets the total number of pupils matched by the search criteria.
    /// Guaranteed to be non-negative.
    /// </summary>
    public int TotalNumberOfResults { get; init; } = 0;
}
