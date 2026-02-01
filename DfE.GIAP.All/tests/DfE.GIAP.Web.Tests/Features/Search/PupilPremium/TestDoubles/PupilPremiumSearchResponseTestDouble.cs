using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Web.Tests.Features.Search.PupilPremium.TestDoubles;
public static class PupilPremiumSearchResponseTestDouble
{
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

    public static PupilPremiumSearchResponse CreateSuccessResponse()
    {
        // Construct a sample learner with basic identity and characteristics
        PupilPremiumLearners learners = new(
            [new(
                UniquePupilNumberTestDoubles.Generate(),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(age: 13),
                    Sex.Female),
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
