using Bogus;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
public static class UniquePupilNumberTestDoubles
{
    public static UniquePupilNumber Generate() => CreateGenerator().Generate();

    public static List<UniquePupilNumber> Generate(int count) => CreateGenerator().Generate(count);

    private static Faker<UniquePupilNumber> CreateGenerator()
    {
        return new Faker<UniquePupilNumber>()
            .CustomInstantiator(f =>
            {
                char prefix = f.Random.Char('A', 'Z'); // Random uppercase letter
                string digits = f.Random.Replace("###########"); // 11 digits
                string upn = $"{prefix}{digits}";
                return new UniquePupilNumber(upn);
            });
    }
}
