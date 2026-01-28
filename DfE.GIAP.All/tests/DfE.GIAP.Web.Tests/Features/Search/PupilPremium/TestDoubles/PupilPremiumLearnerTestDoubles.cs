using Bogus;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;

namespace DfE.GIAP.Web.Tests.Features.Search.PupilPremium.TestDoubles;
public static class PupilPremiumLearnerTestDoubles
{
    public static List<PupilPremiumLearner> FakeMany(int count = 10)
    {
        List<PupilPremiumLearner> learners = [];
        for (int i = 0; i < count; i++)
        {
            learners.Add(CreateFake());
        }
        return learners;
    }

    public static PupilPremiumLearner CreateFake()
    {
        Faker faker = new();

        return new PupilPremiumLearner(
                UniquePupilNumberTestDoubles.Generate(),
                LearnerNameTestDouble.FakeName(faker),
                LearnerCharacteristicsTestDouble.FakeCharacteristics(faker),
                LocalAuthorityCodeTestDoubles.Stub());
    }
}
