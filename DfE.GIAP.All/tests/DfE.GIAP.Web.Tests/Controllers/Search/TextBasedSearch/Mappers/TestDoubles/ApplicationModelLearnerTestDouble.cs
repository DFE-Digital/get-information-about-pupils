using DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;

/// <summary>
/// Provides a reusable test double for constructing <see cref="Learner"/> domain objects.
/// Centralizes learner scaffolding for unit tests to improve readability and reduce duplication.
/// </summary>
public static class ApplicationModelLearnerTestDouble
{
    /// <summary>
    /// Creates a <see cref="LearnerIdentifier"/> from a unique learner number.
    /// Encapsulates identity logic for symbolic traceability in tests.
    /// </summary>
    private static LearnerIdentifier Identifier(string uniqueLearnerNumber) =>
        new(uniqueLearnerNumber);

    /// <summary>
    /// Constructs a <see cref="LearnerName"/> from first and last name.
    /// Supports semantic clarity when asserting name-based behavior.
    /// </summary>
    private static LearnerName Name(string firstname, string surname) =>
        new(firstname, surname);

    /// <summary>
    /// Builds a <see cref="LearnerCharacteristics"/> object from birth date and gender.
    /// Enables characteristic-based filtering and diagnostics in test scenarios.
    /// </summary>
    private static LearnerCharacteristics Characteristics(
        DateTime birthDate,
        LearnerCharacteristics.Gender gender) =>
        new(birthDate, gender);

    /// <summary>
    /// Creates a fully populated <see cref="Learner"/> instance for use in unit tests.
    /// Combines identity, name, and characteristics into a single domain object.
    /// </summary>
    /// <param name="uniqueLearnerNumber">Unique learner identifier for symbolic traceability.</param>
    /// <param name="firstname">First name of the learner.</param>
    /// <param name="surname">Last name of the learner.</param>
    /// <param name="birthDate">Date of birth for age-based diagnostics.</param>
    /// <param name="gender">Gender for demographic filtering and assertions.</param>
    /// <returns>A scaffolded <see cref="Learner"/> domain object.</returns>
    public static Learner Stub(
        string uniqueLearnerNumber,
        string firstname,
        string surname,
        DateTime birthDate,
        LearnerCharacteristics.Gender gender)
    {
        // Construct domain components using helper methods for modularity and reuse
        LearnerIdentifier learnerIdentifier = Identifier(uniqueLearnerNumber);
        LearnerName learnerName = Name(firstname, surname);
        LearnerCharacteristics learnerCharacteristics = Characteristics(birthDate, gender);

        // Compose and return the full Learner object
        return new(learnerIdentifier, learnerName, learnerCharacteristics);
    }
}
