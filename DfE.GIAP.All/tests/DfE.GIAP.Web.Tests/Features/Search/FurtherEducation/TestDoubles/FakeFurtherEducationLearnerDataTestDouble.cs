using System.Diagnostics.CodeAnalysis;
using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;

namespace DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.TestDoubles;

/// <summary>
/// Provides reusable scaffolds for generating randomized learner data using <see cref="Faker"/>.
/// Supports symbolic traceability and deterministic test composition.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FakeFurtherEducationLearnerDataTestDouble
{
    /// <summary>
    /// Generates a valid ULN (Unique Learner Number) as a string.
    /// </summary>
    public static string CreateUniqueLearnerNumber(Faker faker) =>
        faker.Random.Int(1000000000, 2146999999).ToString();

    /// <summary>
    /// Generates a random first name.
    /// </summary>
    public static string CreateFirstname(Faker faker) =>
        faker.Name.FirstName();

    /// <summary>
    /// Generates a random surname.
    /// </summary>
    public static string CreateSurname(Faker faker) =>
        faker.Name.LastName();

    /// <summary>
    /// Generates a birthdate at least 18 years in the past.
    /// </summary>
    public static DateTime CreateBirthDate(Faker faker) =>
        faker.Date.PastOffset(yearsToGoBack: 18).Date;

    /// <summary>
    /// Randomly selects a gender from the available enum values.
    /// </summary>
    public static Gender CreateGender(Faker faker) =>
        faker.PickRandom(
            Gender.Male,
            Gender.Female,
            Gender.Other
        );

    /// <summary>
    /// Generates a full learner identity tuple using randomized values.
    /// </summary>
    /// <returns>
    /// A tuple containing ULN, first name, surname, birthdate, and gender.
    /// </returns>
    public static (string Uln, string FirstName, string Surname, DateTime BirthDate, Gender Gender)
        CreateFurtherEducationLearnerFake(Faker faker)
    {
        LearnerName name = LearnerNameTestDouble.FakeName(faker);
        LearnerCharacteristics characteristics = LearnerCharacteristicsTestDouble.FakeCharacteristics(faker);

        return (
            Uln: CreateUniqueLearnerNumber(faker),
            name.FirstName,
            name.Surname,
            characteristics.BirthDate,
            Gender: characteristics.Sex
        );
    }

}
