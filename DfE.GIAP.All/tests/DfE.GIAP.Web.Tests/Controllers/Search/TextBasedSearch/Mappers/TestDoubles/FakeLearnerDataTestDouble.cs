using System.Diagnostics.CodeAnalysis;
using Bogus;
using DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;

/// <summary>
/// Provides reusable scaffolds for generating randomized learner data using <see cref="Faker"/>.
/// Supports symbolic traceability and deterministic test composition.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FakeLearnerDataTestDouble
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
    public static LearnerCharacteristics.Gender CreateGender(Faker faker) =>
        faker.PickRandom(
            LearnerCharacteristics.Gender.Male,
            LearnerCharacteristics.Gender.Female,
            LearnerCharacteristics.Gender.Other
        );

    /// <summary>
    /// Generates a full learner identity tuple using randomized values.
    /// </summary>
    /// <returns>
    /// A tuple containing ULN, first name, surname, birthdate, and gender.
    /// </returns>
    public static (string Uln, string FirstName, string Surname, DateTime BirthDate, LearnerCharacteristics.Gender Gender)
        CreateLearnerFake(Faker faker) =>
        (
            Uln: CreateUniqueLearnerNumber(faker),
            FirstName: CreateFirstname(faker),
            Surname: CreateSurname(faker),
            BirthDate: CreateBirthDate(faker),
            Gender: CreateGender(faker)
        );
}
