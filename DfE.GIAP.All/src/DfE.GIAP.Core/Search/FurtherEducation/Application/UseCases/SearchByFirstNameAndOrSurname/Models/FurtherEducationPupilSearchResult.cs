using DfE.GIAP.Core.Search.Common.Application.Models;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Model;

namespace DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;

/// <summary>
/// Represents the outcome of a further education pupil search operation.
/// Contains metadata, result payload, and any associated facets.
/// </summary>
public sealed class FurtherEducationPupilSearchResult
{
    /// <summary>
    /// Initializes a new instance with the specified search response status.
    /// </summary>
    /// <param name="status">The status code or enum representing search outcome.</param>
    public FurtherEducationPupilSearchResult(SearchResponseStatus status)
    {
        Status = status;
    }

    /// <summary>
    /// Gets the returned pupil results, if any were found.
    /// </summary>
    public FurtherEducationPupils? PupilResults { get; init; }

    /// <summary>
    /// Gets the facet groupings returned from the search, used for aggregation or filtering context.
    /// </summary>
    public FurtherEducationFacets? FacetedResults { get; init; }

    /// <summary>
    /// Gets the status of the search, indicating success, failure, or other condition.
    /// </summary>
    public SearchResponseStatus Status { get; }

    /// <summary>
    /// Gets the total number of distinct establishments involved in the pupil result set.
    /// This may be used for summary metrics or analytics.
    /// </summary>
    public int TotalNumberOfEstablishments { get; init; }
}
