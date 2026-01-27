using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.Search.Application.Models.Learner.PupilPremium;
public sealed class PupilPremiumLearner : Entity<UniquePupilNumber>
{
    /// <summary>
    /// Gets the learner's full name.
    /// </summary>
    public LearnerName Name { get; }

    /// <summary>
    /// Gets the learner's characteristics, such as date of birth and gender.
    /// </summary>
    public LearnerCharacteristics Characteristics { get; }

    public PupilPremiumLearner(
        UniquePupilNumber upn,
        LearnerName name,
        LearnerCharacteristics learnerCharacteristics) : base(upn)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;

        ArgumentNullException.ThrowIfNull(learnerCharacteristics);
        Characteristics = learnerCharacteristics;
    }
}
