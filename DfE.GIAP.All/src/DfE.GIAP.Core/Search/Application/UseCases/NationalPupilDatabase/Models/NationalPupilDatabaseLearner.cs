using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.Common.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
public sealed class NationalPupilDatabaseLearner : Entity<UniquePupilNumber>
{
    public NationalPupilDatabaseLearner(
        UniquePupilNumber identifier,
        LearnerName name,
        LearnerCharacteristics learnerCharacteristics,
        LocalAuthorityCode localAuthority) : base(identifier)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;

        ArgumentNullException.ThrowIfNull(learnerCharacteristics);
        Characteristics = learnerCharacteristics;
        LocalAuthority = localAuthority;
    }

    public LearnerName Name { get; }
    public LearnerCharacteristics Characteristics { get; }
    public LocalAuthorityCode LocalAuthority { get; }
}
