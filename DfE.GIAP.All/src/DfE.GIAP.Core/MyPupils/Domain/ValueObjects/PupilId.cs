using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class PupilId : ValueObject<PupilId>
{
    private readonly Guid _id;

    public PupilId(Guid id)
    {
        _id = id;
        // TODO need to embed a UniqueIdentifier for the Pupil that is different to its UPN because that's just a presentation concern here
        // Could be removing the UniquePupilnumber from this, and this being a separated ValueObject. The masking would also move into Pupil.
    }

    public string Id => _id.ToString();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
