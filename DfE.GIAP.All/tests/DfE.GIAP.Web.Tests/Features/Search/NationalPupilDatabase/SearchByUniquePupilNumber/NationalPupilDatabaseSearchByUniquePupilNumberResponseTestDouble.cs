using Bogus;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;

namespace DfE.GIAP.Web.Tests.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;
public static class NationalPupilDatabaseSearchByUniquePupilNumberResponseTestDouble
{
    public static NationalPupilDatabaseSearchByUniquePupilNumberResponse Create(
        NationalPupilDatabaseLearners learners,
        int? totalResults = null) => new(learners, totalResults ?? learners.Count);

    public static NationalPupilDatabaseSearchByUniquePupilNumberResponse CreateSuccessResponse()
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

        // Return a success response with the sample learner and facet
        return new NationalPupilDatabaseSearchByUniquePupilNumberResponse(learners, learners.Count);
    }
}
