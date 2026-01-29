using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;

/// <summary>
/// Provides test doubles for <see cref="FurtherEducationLearners"/> used in unit tests.
/// Enables deterministic testing of search result handling and downstream mapping logic.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FurtherEducationLearnersTestDouble
{
    /// <summary>
    /// Creates a stubbed <see cref="FurtherEducationLearners"/> instance populated with a randomized number of learner objects.
    /// Each learner is generated using <see cref="FurtherEducationLearnerTestDouble.Create"/>, allowing synthetic test data
    /// to simulate realistic search results.
    /// </summary>
    /// <returns>A populated <see cref="FurtherEducationLearners"/> object for use in test scenarios.</returns>
    public static FurtherEducationLearners Stub()
    {
        List<FurtherEducationLearner> learners = [];

        for (int i = 0; i < new Bogus.Faker().Random.Int(1, 10); i++)
        {
            learners.Add(
                FurtherEducationLearnerTestDouble.Fake()); // Generate synthetic learner instance
        }

        return new FurtherEducationLearners(learners);
    }

    /// <summary>
    /// Returns an empty <see cref="FurtherEducationLearners"/> instance.
    /// Useful for testing edge cases, empty result handling, and fallback logic.
    /// </summary>
    public static FurtherEducationLearners EmptyStub() => new();
}
