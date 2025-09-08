using DfE.GIAP.Core.Search.Application.Models.Filter;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides a factory for creating fake <see cref="FilterRequest"/> instances
/// for use in unit tests. This avoids the need to manually construct test data
/// and ensures generated values are realistic but non-deterministic.
/// </summary>
internal static class FilterRequestFake
{
    /// <summary>
    /// Creates a new <see cref="FilterRequest"/> populated with random but plausible values
    /// using the <c>Bogus</c> data generation library.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The generated filter name is a random job type (e.g., "Engineer", "Manager"),
    /// and the filter values are two random job titles.
    /// </para>
    /// <para>
    /// This is intended for test scenarios where the specific filter values are not important,
    /// but a valid <see cref="FilterRequest"/> object is required.
    /// </para>
    /// </remarks>
    /// <returns>
    /// A <see cref="FilterRequest"/> instance with randomly generated name and values.
    /// </returns>
    public static FilterRequest Create()
    {
        // Instantiate a Bogus faker for generating realistic fake data.
        Bogus.Faker faker = new();

        // Construct and return a FilterRequest with:
        // - A random job type as the filter name
        // - Two random job titles as the filter values
        return new FilterRequest(
            faker.Name.JobType(),
            [faker.Name.JobTitle(), faker.Name.JobTitle()]
        );
    }
}
