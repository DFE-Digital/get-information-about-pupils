using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.SharedTests.TestDoubles.Learner;
public static class PupilPremiumLearnerTestDoubles
{
    public static List<PupilPremiumLearner> FakeMany(int count = 10)
    {
        List<PupilPremiumLearner> learners = [];
        for (int i = 0; i < count; i++)
        {
            learners.Add(Fake());
        }
        return learners;
    }

    public static PupilPremiumLearner Fake()
    {
        Faker faker = new();

        return new PupilPremiumLearner(
                UniquePupilNumberTestDoubles.Generate(),
                LearnerNameTestDouble.FakeName(faker),
                LearnerCharacteristicsTestDouble.FakeCharacteristics(faker),
                LocalAuthorityCodeTestDoubles.Stub());
    }

    public static PupilPremiumLearner FakeWithMiddleName(string? middleName)
    {
        Faker faker = new();
        LearnerName generatedName = LearnerNameTestDouble.FakeName(faker);

        return new PupilPremiumLearner(
                UniquePupilNumberTestDoubles.Generate(),
                new LearnerName(generatedName.FirstName, middleName!, generatedName.Surname),
                LearnerCharacteristicsTestDouble.FakeCharacteristics(faker),
                LocalAuthorityCodeTestDoubles.Stub());
    }
}
