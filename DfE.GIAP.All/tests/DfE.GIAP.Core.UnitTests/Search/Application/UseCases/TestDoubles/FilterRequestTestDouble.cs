using DfE.GIAP.Core.Search.Application.Models.Filter;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;

/// <summary>
/// Provides a stubbed <see cref="FilterRequest"/> instance for use in unit tests.
/// Simulates realistic filter input using randomized job-related values,
/// enabling deterministic testing of filter construction and adapter behavior.
/// </summary>
internal class FilterRequestTestDouble
{
    /// <summary>
    /// Generates a fake <see cref="FilterRequest"/> with a randomized filter name
    /// and two job title values using Bogus for synthetic data generation.
    /// Useful for testing filter expression builders and search adapter logic.
    /// </summary>
    public static FilterRequest Fake()
    {
        Bogus.Faker faker = new();

        return new FilterRequest(
            filterName: faker.Name.JobType(),                               // Simulated filter name
            filterValues: [faker.Name.JobTitle(), faker.Name.JobTitle()]    // Simulated filter values
        );
    }
}
