using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class PupilId : ValueObject<PupilId>
{
    private const string MaskedPupilMarker = "*************";
    public PupilId(
        UniquePupilNumber id,
        bool isUniquePupilIdentifierMasked)
    {

        Id = Guid.NewGuid().ToString(); // TODO need to embed a UniqueIdentifier for the Pupil that is different to its UPN because that's just a presentation concern here

        Upn = isUniquePupilIdentifierMasked ?
            MaskedPupilMarker :
                id.Value.ToString();
    }

    public string Upn { get; init; }
    public string Id { get; init; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
