using Bogus;
using DfE.GIAP.SharedTests.Common;

namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
public static class AzureFurtherEducationSearchResponseDtoTestDoubles
{
    private static readonly string[] ValidSexes = ["M", "F"];

    public static List<AzureFurtherEducationSearchResponseDto> Generate(int count = 10)
    {
        return CreateFaker().Generate(count);
    }

    private static Faker<AzureFurtherEducationSearchResponseDto> CreateFaker()
    {

        Faker<AzureFurtherEducationSearchResponseDto> faker = new Faker<AzureFurtherEducationSearchResponseDto>()
            .RuleFor(t => t.Score, f => f.Random.Double(0, 1))
            .RuleFor(t => t.ULN, f => f.Random.Int(1000000000, 2146999999).ToString())
            .RuleFor(t => t.Forename, f => f.Name.FirstName())
            .RuleFor(t => t.Surname, f => f.Name.LastName())
            .RuleFor(t => t.Sex, f =>
            {
                return f.PickRandom(ValidSexes);
            })
            .RuleFor(t => t.Gender, f =>
            {
                return f.PickRandom(ValidSexes);
            })
            .RuleFor(t => t.DOB, f => DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(f.Random.Number(1, 25)).ToString("yyyy-MM-dd"))
            .RuleFor(t => t.LocalAuthority, f => f.Random.Number(100, 999).ToString())
            .RuleFor(t => t.id, f => f.Random.Guid().ToString());

        return faker;
    }
}
