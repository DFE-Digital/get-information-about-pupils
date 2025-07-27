using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using Fare;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class UniquePupilNumberTestDoubles
{
    public static UniquePupilNumber Generate() => Generate(1).Single();

    public static List<UniquePupilNumber> Generate(int count)
    {
        Xeger xeger = new(
            UniquePupilNumberValidator.UpnRegex().ToString(),
            new Random());

        List<UniquePupilNumber> upns = [];

        for (int i = 0; i < count; i++)
        {
            UniquePupilNumber uniquePupilNumber = new(xeger.Generate());
            upns.Add(uniquePupilNumber);
        }

        return upns;
    }
    
}
