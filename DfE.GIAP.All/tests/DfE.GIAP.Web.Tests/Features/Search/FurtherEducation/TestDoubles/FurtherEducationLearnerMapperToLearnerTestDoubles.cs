using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Domain.Search.Learner;

namespace DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.TestDoubles;

/// <summary>
/// Provides a reusable test double for mapping between application and domain Learner models.
/// Centralizes mock setup for unit tests involving IMapper usage.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class FurtherEducationLearnerMapperToLearnerTestDoubles
{
    // Static mock instance to be reused across test invocations.
    // This avoids redundant instantiation and supports centralized setup.
    private static readonly Mock<IMapper<FurtherEducationLearner, Learner>> _mock = new();

    /// <summary>
    /// Returns the shared mock instance for direct configuration or verification.
    /// </summary>
    public static Mock<IMapper<FurtherEducationLearner, Learner>> Mock() => _mock;

    /// <summary>
    /// Configures the mock to return specific domain Learner instances
    /// when given corresponding application Learner inputs.
    /// Useful for simulating multiple deterministic mappings in tests.
    /// </summary>
    /// <param name="learners">
    /// A sequence of tuples pairing application model learners with expected domain learners.
    /// </param>
    /// <returns>
    /// The configured mock instance with mapping setups applied.
    /// </returns>
    public static Mock<IMapper<FurtherEducationLearner, Learner>> MockForMultiple(
        IEnumerable<(FurtherEducationLearner, Learner)> learners)
    {
        // Iterate through each learner pair and configure the mock to return
        // the expected domain model when the corresponding application model is mapped.
        foreach ((FurtherEducationLearner applicationModelLearner, Learner domainLearner) in learners)
        {
            _mock.Setup(mapper =>
                mapper.Map(applicationModelLearner)).Returns(domainLearner);
        }

        return _mock;
    }

    public static Mock<IMapper<
        FurtherEducationLearner, Learner>> MockForApplicationModelToDomain(
    IEnumerable<FurtherEducationLearner> applicationModelLearners)
    {
        foreach (FurtherEducationLearner learner in applicationModelLearners)
        {
            _mock.Setup(mapper => mapper.Map(learner))
                .Returns(new Learner() { Id = learner.Identifier.UniqueLearnerNumber.ToString() });
        }

        return _mock;
    }
}
