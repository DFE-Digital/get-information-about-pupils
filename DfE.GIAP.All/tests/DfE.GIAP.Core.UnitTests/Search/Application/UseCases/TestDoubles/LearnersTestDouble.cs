using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;

/// <summary>
/// Provides test doubles for <see cref="Learners"/> used in unit tests.
/// Enables deterministic testing of search result handling and downstream mapping logic.
/// </summary>
[ExcludeFromCodeCoverage]
public static class LearnersTestDouble
{
    /// <summary>
    /// Creates a stubbed <see cref="Learners"/> instance populated with a randomized number of learner objects.
    /// Each learner is generated using <see cref="LearnerTestDouble.Create"/>, allowing synthetic test data
    /// to simulate realistic search results.
    /// </summary>
    /// <returns>A populated <see cref="Learners"/> object for use in test scenarios.</returns>
    public static Learners Stub()
    {
        List<Learner> learners = [];

        for (int i = 0; i < new Bogus.Faker().Random.Int(1, 10); i++)
        {
            learners.Add(
                LearnerTestDouble.Fake()); // Generate synthetic learner instance
        }

        return new Learners(learners);
    }

    /// <summary>
    /// Returns an empty <see cref="Learners"/> instance.
    /// Useful for testing edge cases, empty result handling, and fallback logic.
    /// </summary>
    public static Learners EmptyStub() => new();
}
