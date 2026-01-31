using System.Diagnostics.CodeAnalysis;
using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.SharedTests.TestDoubles.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.TestDoubles;

/// <summary>
/// Provides a test double for <see cref="FurtherEducationLearner"/> used in unit tests.
/// Generates realistic synthetic learner data using Bogus to support deterministic testing
/// of search result mapping, filtering, and adapter behavior.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FurtherEducationLearnerTestDouble
{
    /// <summary>
    /// Constructs a fully populated <see cref="FurtherEducationLearner"/> instance using randomized data.
    /// Enables symbolic traceability and onboarding clarity for test scenarios involving learner entities.
    /// </summary>
    public static FurtherEducationLearner Fake()
    {
        // Instantiate a Bogus faker for generating realistic fake data
        Faker faker = new();

        FurtherEducationLearnerIdentifier learnerIdentifier = FurtherEducationLearnerIdentifierTestDouble.FakeIdentifier(faker);
        LearnerName learnerName = LearnerNameTestDouble.FakeName(faker);
        LearnerCharacteristics learnerCharacteristics = LearnerCharacteristicsTestDouble.FakeCharacteristics(faker);

        return new(learnerIdentifier, learnerName, learnerCharacteristics);
    }
}
