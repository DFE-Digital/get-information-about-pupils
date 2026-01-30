using Bogus;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.Web.Tests.Features.Search.Shared.TestDoubles;

namespace DfE.GIAP.Web.Tests.Features.Search.NationalPupilDatabase.TestDoubles;
internal static class NationalPupilDatabaseSearchResponseTestDoubles
{
    public static NationalPupilDatabaseSearchResponse Create(
        NationalPupilDatabaseLearners learners,
        SearchFacets facets,
        SearchResponseStatus status = SearchResponseStatus.Success,
        int? totalResults = null) => new(status, totalResults ?? learners.Count)
        {
            LearnerSearchResults = learners,
            FacetedResults = facets
        };

    public static NationalPupilDatabaseSearchResponse CreateSuccessResponse()
    {
        // Construct a sample learner with basic identity and characteristics
        Faker faker = new();

        NationalPupilDatabaseLearners learners = new(
            [new(
                UniquePupilNumberTestDoubles.Generate(),
                LearnerNameTestDouble.FakeName(faker),
                LearnerCharacteristicsTestDouble.FakeCharacteristics(faker),
                LocalAuthorityCodeTestDoubles.Stub()
                )
            ]
        );

        // Construct a sample facet group with one region facet
        SearchFacets facets = SearchFacetsTestDouble.CreateSingleFacetGroup("Region", "North", 10);

        // Return a success response with the sample learner and facet
        return new NationalPupilDatabaseSearchResponse(SearchResponseStatus.Success, learners.Count)
        {
            LearnerSearchResults = learners,
            FacetedResults = facets
        };
    }
}
