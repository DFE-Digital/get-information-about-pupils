using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.SharedTests.TestDoubles.Learner;
public static class NationalPupilDatabaseLearnerTestDoubles
{
    public static List<NationalPupilDatabaseLearner> FakeMany(int count = 10)
    {
        List<NationalPupilDatabaseLearner> learners = [];
        for (int i = 0; i < count; i++)
        {
            learners.Add(Fake());
        }
        return learners;
    }

    public static NationalPupilDatabaseLearner Fake()
    {
        Faker faker = new();

        return new NationalPupilDatabaseLearner(
                UniquePupilNumberTestDoubles.Generate(),
                LearnerNameTestDouble.FakeName(faker),
                LearnerCharacteristicsTestDouble.FakeCharacteristics(faker),
                LocalAuthorityCodeTestDoubles.Stub());
    }

    public static NationalPupilDatabaseLearner FakeWithMiddleName(string? middleName)
    {
        Faker faker = new();
        LearnerName generatedName = LearnerNameTestDouble.FakeName(faker);

        return new NationalPupilDatabaseLearner(
                UniquePupilNumberTestDoubles.Generate(),
                new LearnerName(generatedName.FirstName, middleName!, generatedName.Surname),
                LearnerCharacteristicsTestDouble.FakeCharacteristics(faker),
                LocalAuthorityCodeTestDoubles.Stub());
    }
}
