using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.Learner;
public static class LearnerCharacteristicsTestDouble
{
    /// <summary>
    /// Generates a fake <see cref="LearnerCharacteristics"/> object with a randomized birth date
    /// and gender selection. Supports testing of demographic filters and facet mapping.
    /// </summary>
    public static LearnerCharacteristics FakeCharacteristics(Faker faker) =>
        new(
            birthDate: CreateBirthDate(faker),
            sex: CreateSex(faker));


    /// <summary>
    /// Generates a birthdate at least 18 years in the past.
    /// </summary>
    public static DateTime CreateBirthDate(Faker faker) =>
        faker.Date.PastOffset(yearsToGoBack: 18).Date;

    /// <summary>
    /// Randomly selects a gender from the available enum values.
    /// </summary>
    public static Sex CreateSex(Faker faker) =>
        faker.PickRandom(
            Sex.Male,
            Sex.Female,
            Sex.Unknown
        );
}
