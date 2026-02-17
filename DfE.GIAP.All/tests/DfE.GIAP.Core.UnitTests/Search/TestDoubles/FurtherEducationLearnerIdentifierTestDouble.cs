using Bogus;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;
public static class FurtherEducationLearnerIdentifierTestDouble
{
    /// <summary>
    /// Generates a fake <see cref="FurtherEducationUniqueLearnerIdentifier"/> with a randomized ULN (Unique Learner Number).
    /// Ensures numeric format consistency for downstream validation and traceability.
    /// </summary>
    public static FurtherEducationUniqueLearnerIdentifier FakeIdentifier(Faker faker) =>
        new(
            uniqueLearnerNumber: faker.Random.Int(1000000000, 2146999999).ToString());
}
