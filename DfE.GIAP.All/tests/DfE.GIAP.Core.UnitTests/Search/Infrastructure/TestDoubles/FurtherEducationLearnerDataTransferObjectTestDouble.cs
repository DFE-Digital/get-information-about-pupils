using Bogus;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides a factory for creating fake <see cref="FurtherEducationLearnerDataTransferObject"/> instances
/// for use in unit tests. Uses the <c>Bogus</c> library to generate realistic
/// but randomised data for learner identifiers, names, and characteristics.
/// </summary>
internal static class FurtherEducationLearnerDataTransferObjectTestDouble
{
    /// <summary>
    /// Creates a new <see cref="FurtherEducationLearnerDataTransferObject"/> populated with random but plausible values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The generated learner will have:
    /// <list type="bullet">
    /// <item>A random unique learner number (6 digits)</item>
    /// <item>A random first name and surname</item>
    /// <item>A birth date approximately 18 years in the past</item>
    /// <item>A randomly selected gender from the <see cref="Gender"/> enum</item>
    /// </list>
    /// </para>
    /// This is intended for test scenarios where the specific learner details
    /// are not important, but a valid <see cref="FurtherEducationLearnerDataTransferObject"/> object is required.
    /// </remarks>
    /// <returns>
    /// A <see cref="FurtherEducationLearnerDataTransferObject"/> instance with randomly generated data.
    /// </returns>
    public static FurtherEducationLearnerDataTransferObject Fake()
    {
        // Instantiate a Bogus faker for generating realistic fake data
        Faker faker = new();

        // Return the fully constructed LearnerDataTransferObject
        return new FurtherEducationLearnerDataTransferObject()
        {
            ULN = faker.Random.Int(1000000000, 2146999999).ToString(),
            Surname = faker.Name.LastName(),
            Forename = faker.Name.FirstName(),
            DOB = faker.Date.PastOffset(yearsToGoBack: 18).Date,
            Gender = faker.PickRandom(Gender.Male, Gender.Female, Gender.Other).ToString(),
            Sex = faker.PickRandom(Gender.Male, Gender.Female, Gender.Other).ToString()
        };
    }
}
