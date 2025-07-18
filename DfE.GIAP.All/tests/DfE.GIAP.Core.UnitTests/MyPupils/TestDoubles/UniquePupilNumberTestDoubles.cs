using System.Text;
using Bogus;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
public static class UniquePupilNumberTestDoubles
{
    public static UniquePupilNumber Generate() => CreateGenerator().Generate();

    public static List<UniquePupilNumber> Generate(int count) => CreateGenerator().Generate(count);

    private static Faker<UniquePupilNumber> CreateGenerator() // TODO could we just input the UpnRegex to the generator?
    {
        return new Faker<UniquePupilNumber>()
            .CustomInstantiator(f =>
            {
                StringBuilder upnBuilder = new();
                char prefix = f.PickRandom('A', 'T'); // 'A' for permanent, 'T' for temporary
                upnBuilder.Append(prefix);

                string digits = f.Random.ReplaceNumbers("###########"); // 11 digits
                upnBuilder.Append(digits);

                char checkDigit = CalculateCheckDigit(upnBuilder.ToString());
                upnBuilder.Append(checkDigit);

                return new UniquePupilNumber(upnBuilder.ToString());
            });
    }

    private static char CalculateCheckDigit(string input)
    {
        // Simple mod-36 check digit (0-9, A-Z)
        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int sum = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            int value = char.IsDigit(c) ? c - '0' : c - 'A' + 10;
            sum += value * (i + 1); // weighted sum
        }

        return charset[sum % 36];
    }

}
