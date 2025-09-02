using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Repositories;
public sealed class MyPupils
{
    public MyPupils(UniquePupilNumbers pupilNumbers)
    {
        ArgumentNullException.ThrowIfNull(pupilNumbers);
        Pupils = pupilNumbers;
    }

    public UniquePupilNumbers Pupils { get; }
}
