using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Mappers.TestDoubles;
public static class PupilPremiumSearchByKeyWordsResponseTestDouble
{
    /// <summary>
    /// Creates a fully populated response with learners and facets.
    /// Useful for simulating successful search results in unit tests.
    /// </summary>
    /// <param name="learners">Learner collection to include in the response.</param>
    /// <param name="facets">Facet overlays for filtering and UI diagnostics.</param>
    /// <param name="status">Search status (default: Success).</param>
    /// <param name="totalResults">Total number of matched learners (default: learners.Count).</param>
    /// <returns>A scaffolded <see cref="PupilPremiumSearchResponse"/> object.</returns>
    public static PupilPremiumSearchResponse Create(
        PupilPremiumLearners learners,
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
    /// <returns>A success <see cref="FurtherEducationSearchResponse"/> with sample data.</returns>
    public static PupilPremiumSearchResponse CreateSuccessResponse()
    {
        // Construct a sample learner with basic identity and characteristics
        PupilPremiumLearners learners = new(
            [new(
                UniquePupilNumberTestDoubles.Generate(),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(age: 13),
                    Gender.Female),
                new LocalAuthorityCode(100)
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
        return new PupilPremiumSearchResponse(SearchResponseStatus.Success, learners.Count)
        {
            LearnerSearchResults = learners,
            FacetedResults = facets
        };
    }
}
