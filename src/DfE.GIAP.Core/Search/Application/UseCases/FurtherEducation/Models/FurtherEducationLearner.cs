using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

/// <summary>
/// Represents a learner in the Further Education domain.
/// Encapsulates identity, name, and core characteristics.
/// </summary>
public sealed class FurtherEducationLearner : Entity<FurtherEducationUniqueLearnerIdentifier>
{
    /// <summary>
    /// Gets the learner's full name.
    /// </summary>
    public LearnerName Name { get; }

    /// <summary>
    /// Gets the learner's characteristics, such as date of birth and gender.
    /// </summary>
    public LearnerCharacteristics Characteristics { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FurtherEducationLearner"/> class.
    /// </summary>
    /// <param name="learnerIdentifier">The unique identifier for the learner.</param>
    /// <param name="name">The learner's name.</param>
    /// <param name="learnerCharacteristics">The learner's characteristics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="name"/> or <paramref name="learnerCharacteristics"/> is null.
    /// </exception>
    public FurtherEducationLearner(
        FurtherEducationUniqueLearnerIdentifier learnerIdentifier,
        LearnerName name,
        LearnerCharacteristics learnerCharacteristics) : base(learnerIdentifier)
    {
        Name = name ??
            throw new ArgumentNullException(nameof(name));
        Characteristics = learnerCharacteristics ??
            throw new ArgumentNullException(nameof(learnerCharacteristics));
    }
}
