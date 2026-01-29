using Bogus;
using DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.SharedTests.TestDoubles.Learner;
public static class LearnerNameTestDouble
{
    /// <summary>
    /// Generates a fake <see cref="LearnerName"/> using randomized first and last names.
    /// Useful for testing name-based sorting, filtering, and display logic.
    /// </summary>
    public static LearnerName FakeName(Faker faker) =>
        new(
            firstName: faker.Name.FirstName(),
            middleName: faker.PickRandom(faker.Name.FirstName(), string.Empty),
            surname: faker.Name.LastName());
}
