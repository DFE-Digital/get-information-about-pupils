using Bogus;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using static DfE.GIAP.Core.Search.Application.Models.Learner.LearnerCharacteristics;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;

/// <summary>
/// Provides a test double for <see cref="Learner"/> used in unit tests.
/// Generates realistic synthetic learner data using Bogus to support deterministic testing
/// of search result mapping, filtering, and adapter behavior.
/// </summary>
public static class LearnerTestDouble
{
    /// <summary>
    /// Generates a fake <see cref="LearnerIdentifier"/> with a randomized ULN (Unique Learner Number).
    /// Ensures numeric format consistency for downstream validation and traceability.
    /// </summary>
    private static LearnerIdentifier FakeIdentifier(Faker faker) =>
        new(
            uniqueLearnerNumber: faker.Random.Int(1000000000, 2146999999).ToString());

    /// <summary>
    /// Generates a fake <see cref="LearnerName"/> using randomized first and last names.
    /// Useful for testing name-based sorting, filtering, and display logic.
    /// </summary>
    private static LearnerName FakeName(Faker faker) =>
        new(
            firstName: faker.Name.FirstName(),
            surname: faker.Name.LastName());

    /// <summary>
    /// Generates a fake <see cref="LearnerCharacteristics"/> object with a randomized birth date
    /// and gender selection. Supports testing of demographic filters and facet mapping.
    /// </summary>
    private static LearnerCharacteristics FakeCharacteristics(Faker faker) =>
        new(
            birthDate: faker.Date.PastOffset(yearsToGoBack: 18).Date,
            gender: faker.PickRandom(Gender.Male, Gender.Female, Gender.Other));

    /// <summary>
    /// Constructs a fully populated <see cref="Learner"/> instance using randomized data.
    /// Enables symbolic traceability and onboarding clarity for test scenarios involving learner entities.
    /// </summary>
    public static Learner Fake()
    {
        // Instantiate a Bogus faker for generating realistic fake data
        Faker faker = new();

        LearnerIdentifier learnerIdentifier = FakeIdentifier(faker);
        LearnerName learnerName = FakeName(faker);
        LearnerCharacteristics learnerCharacteristics = FakeCharacteristics(faker);

        return new(learnerIdentifier, learnerName, learnerCharacteristics);
    }
}
