using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.Common.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
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
    public LocalAuthorityCode LocalAuthority { get; }

    public PupilPremiumLearner(
        UniquePupilNumber upn,
        LearnerName name,
        LearnerCharacteristics learnerCharacteristics,
        LocalAuthorityCode localAuthority) : base(upn)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;

        ArgumentNullException.ThrowIfNull(learnerCharacteristics);
        Characteristics = learnerCharacteristics;
        LocalAuthority = localAuthority;
    }
}
