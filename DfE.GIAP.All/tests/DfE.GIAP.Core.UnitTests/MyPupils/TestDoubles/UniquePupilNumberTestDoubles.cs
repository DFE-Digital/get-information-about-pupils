using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
public static class UniquePupilNumberTestDoubles
{
    public static UniquePupilNumber Generate()
    {
        return new("A12345678901"); // TODO Regex on structure
    }
}
