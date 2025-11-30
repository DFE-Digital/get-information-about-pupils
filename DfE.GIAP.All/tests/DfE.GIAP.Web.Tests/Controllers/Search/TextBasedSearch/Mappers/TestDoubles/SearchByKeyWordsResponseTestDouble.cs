using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Response;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;

/// <summary>
/// Provides scaffolds for creating <see cref="SearchResponse"/> objects for unit testing.
/// Enables deterministic construction of learner results and facet overlays for search scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SearchByKeyWordsResponseTestDouble
{
    /// <summary>
    /// Creates a fully populated response with learners and facets.
    /// Useful for simulating successful search results in unit tests.
    /// </summary>
    /// <param name="learners">Learner collection to include in the response.</param>
    /// <param name="facets">Facet overlays for filtering and UI diagnostics.</param>
    /// <param name="status">Search status (default: Success).</param>
    /// <param name="totalResults">Total number of matched learners (default: learners.Count).</param>
    /// <returns>A scaffolded <see cref="SearchResponse"/> object.</returns>
    public static SearchResponse Create(
        Learners learners,
        SearchFacets facets,
        SearchResponseStatus status = SearchResponseStatus.Success,
        int? totalResults = null) =>
            new(status, totalResults ?? learners.Count)
            {
                LearnerSearchResults = learners,
                FacetedResults = facets
            };

    /// <summary>
    /// Creates a predefined success response with one learner and one facet.
    /// Useful for quick positive-path testing without custom setup.
    /// </summary>
    /// <returns>A success <see cref="SearchResponse"/> with sample data.</returns>
    public static SearchResponse CreateSuccessResponse()
    {
        // Construct a sample learner with basic identity and characteristics
        Learners learners = new(
            [new(
                new LearnerIdentifier("1234567890"),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    new DateTime(2005, 6, 1),
                    LearnerCharacteristics.Gender.Female)
                )
            ]
        );

        // Construct a sample facet group with one region facet
        SearchFacets facets = new(
            [
                new("Region",
                [
                    new FacetResult("North", 10)
                ])
            ]
        );

        // Return a success response with the sample learner and facet
        return new SearchResponse(SearchResponseStatus.Success, learners.Count)
        {
            LearnerSearchResults = learners,
            FacetedResults = facets
        };
    }
}
