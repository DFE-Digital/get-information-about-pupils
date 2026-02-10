using Bogus;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Web.Tests.Features.Search.NationalPupilDatabase.SearchByName;
internal static class NationalPupilDatabaseSearchByNameResponseTestDoubles
{
    public static NationalPupilDatabaseSearchByNameResponse Create(
        NationalPupilDatabaseLearners learners,
        SearchFacets facets,
        SearchResponseStatus status = SearchResponseStatus.Success,
        int? totalResults = null) => new(learners, facets, totalResults ?? learners.Count);

    public static NationalPupilDatabaseSearchByNameResponse CreateSuccessResponse()
    {
        // Construct a sample learner with basic identity and characteristics
        Faker faker = new();

        NationalPupilDatabaseLearners learners = new(
            [
                new(
                    UniquePupilNumberTestDoubles.Generate(),
                    LearnerNameTestDouble.FakeName(faker),
                    LearnerCharacteristicsTestDouble.FakeCharacteristics(faker),
                    LocalAuthorityCodeTestDoubles.Stub())
            ]
        );

        // Construct a sample facet group with one region facet
        SearchFacets facets = SearchFacetsTestDouble.CreateSingleFacetGroup("Region", "North", 10);

        // Return a success response with the sample learner and facet
        return new NationalPupilDatabaseSearchByNameResponse(learners, facets, learners.Count);
    }
}
