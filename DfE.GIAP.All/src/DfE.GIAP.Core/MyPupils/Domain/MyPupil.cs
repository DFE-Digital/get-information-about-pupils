using DfE.GIAP.Core.Common.Domain.Contracts;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain;
public sealed class MyPupil : ValueObject<MyPupil>
{
    public MyPupil(MyPupilIdentifier id)
    {
        Id = id;
    }

    public MyPupilIdentifier Id { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
