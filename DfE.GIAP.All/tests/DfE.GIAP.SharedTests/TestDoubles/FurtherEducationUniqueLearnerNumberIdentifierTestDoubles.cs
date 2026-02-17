using Bogus;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class FurtherEducationUniqueLearnerNumberIdentifierTestDoubles
{
    /// <summary>
    /// Generates a valid ULN (Unique Learner Number) as a string.
    /// </summary>
    public static string CreateUniqueLearnerNumber(Faker faker) =>
        faker.Random.Int(1000000000, 2146999999).ToString();
}
