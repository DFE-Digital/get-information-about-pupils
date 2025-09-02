using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class MyPupilsTestDoubles
{
    public static Core.MyPupils.Application.Repositories.MyPupils Default()
        => new(UniquePupilNumbers.Create(
                    uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 10)));

    public static Core.MyPupils.Application.Repositories.MyPupils Create(UniquePupilNumbers uniquePupilNumbers)
    {
        return new(uniquePupilNumbers);
    }
}
