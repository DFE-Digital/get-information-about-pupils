using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Repositories;
public record MyPupils
{
    public MyPupils(UniquePupilNumbers pupilNumbers)
    {
        Pupils = pupilNumbers
            ?? new UniquePupilNumbers([]);
    }

    public UniquePupilNumbers Pupils { get; init; }
}
