using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.SearchByName;

/// <summary>
/// Provides scaffolds for creating <see cref="FurtherEducationSearchByNameResponse"/> objects for unit testing.
/// Enables deterministic construction of learner results and facet overlays for search scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FurtherEducationSearchByNameResponseTestDouble
{
    /// <summary>
    /// Creates a fully populated response with learners and facets.
    /// Useful for simulating successful search results in unit tests.
    /// </summary>
    /// <param name="learners">Learner collection to include in the response.</param>
    /// <param name="facets">Facet overlays for filtering and UI diagnostics.</param>
    /// <param name="status">Search status (default: Success).</param>
    /// <param name="totalResults">Total number of matched learners (default: learners.Count).</param>
    /// <returns>A scaffolded <see cref="FurtherEducationSearchByNameResponse"/> object.</returns>
    public static FurtherEducationSearchByNameResponse Create(
        FurtherEducationLearners learners,
        SearchFacets facets,
        int? totalResults = null) =>
            new(learners, facets, totalResults ?? learners.Count);

    /// <summary>
    /// Creates a predefined success response with one learner and one facet.
    /// Useful for quick positive-path testing without custom setup.
    /// </summary>
    /// <returns>A success <see cref="FurtherEducationSearchByNameResponse"/> with sample data.</returns>
    public static FurtherEducationSearchByNameResponse CreateSuccessResponse()
    {
        // Construct a sample learner with basic identity and characteristics
        FurtherEducationLearners learners = new(
            [new(
                new FurtherEducationLearnerIdentifier("1234567890"),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    new DateTime(2005, 6, 1),
                    Sex.Female)
                )
            ]
        );

        // Construct a sample facet group with one region facet
        SearchFacets facets = SearchFacetsTestDouble.CreateSingleFacetGroup("Region", "North", 10);

        // Return a success response with the sample learner and facet
        return new FurtherEducationSearchByNameResponse(learners, facets, learners.Count);
    }
}
