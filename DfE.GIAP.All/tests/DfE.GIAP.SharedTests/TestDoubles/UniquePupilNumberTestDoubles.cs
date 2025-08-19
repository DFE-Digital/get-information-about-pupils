using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using Fare;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class UniquePupilNumberTestDoubles
{
    public static UniquePupilNumber Generate() => Generate(1).Single();

    public static List<UniquePupilNumber> Generate(int count)
    {
        Xeger type1Generator = new(
            UniquePupilNumberValidator.UPN_TYPE_1.ToString(),
            new Random());

        Xeger type2Generator = new(
            UniquePupilNumberValidator.UPN_TYPE_2.ToString(),
            new Random());

        List<UniquePupilNumber> upns = [];

        for (int i = 0; i < count; i++)
        {
            UniquePupilNumber uniquePupilNumber = new(i % 2 == 0 ? type1Generator.Generate() : type2Generator.Generate());
            upns.Add(uniquePupilNumber);
        }

        return upns;
    }


    public static List<string> GenerateAsValues(int count) => Generate(count).Select(t => t.Value).ToList();
}
