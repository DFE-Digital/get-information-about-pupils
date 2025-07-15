using DfE.GIAP.Core.Common.Domain.Contracts;
using DfE.GIAP.Core.Common.Domain.Pupil;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Entities;

public sealed class MyPupil : Entity<MyPupilIdentifier>
{
    private readonly Pupil _pupil;

    public MyPupil(MyPupilIdentifier identifier, Pupil pupil)
        : base(identifier)
    {
        _pupil = pupil;
    }

    // TODO expose fields that extract and format out of Pupil
}
