using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Response;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;

/// <summary>
/// Provides scaffolds for creating <see cref="SearchByKeyWordsResponse"/> objects for unit testing.
/// Supports deterministic construction of learners and facet overlays.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SearchByKeyWordsResponseTestDouble
{
    /// <summary>
    /// Creates a response with specified learners and facets.
    /// </summary>
    /// <param name="learners">Learner collection to include in the response.</param>
    /// <param name="facets">Facet overlays for filtering and UI diagnostics.</param>
    /// <param name="status">Search status (default: Success).</param>
    /// <param name="totalResults">Total number of matched learners (default: learners.Count).</param>
    /// <returns>A scaffolded <see cref="SearchByKeyWordsResponse"/> object.</returns>
    public static SearchByKeyWordsResponse Create(
        Learners learners,
        SearchFacets facets,
        SearchResponseStatus status = SearchResponseStatus.Success,
        int? totalResults = null) =>
            new(status, totalResults ?? learners.Count)
            {
                LearnerSearchResults = learners,
                FacetedResults = facets
            };
}
