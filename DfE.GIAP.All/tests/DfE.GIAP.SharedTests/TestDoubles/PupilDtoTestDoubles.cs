using Bogus;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class PupilDtoTestDoubles
{
    public static PupilDto Generate() => Generate(1).Pupils.Single();

    public static PupilDtos Generate(int count)
    {
        List<PupilDto> pupilDtos = CreateGenerator().Generate(count);
        return PupilDtos.Create(pupilDtos);
    }

    public static PupilDtos GenerateWithUniquePupilNumbers(IEnumerable<UniquePupilNumber> uniquePupilNumbers)
    {
        Faker<PupilDto> generator = CreateGenerator();
        List<PupilDto> output = [];

        uniquePupilNumbers.ToList().ForEach(upn =>
        {
            PupilDto generatedDefault = generator.Generate();
            PupilDto outputPupil = new()
            {
                UniquePupilNumber = upn.Value,
                Sex = generatedDefault.Sex,
                Surname = generatedDefault.Surname,
                Forename = generatedDefault.Forename,
                DateOfBirth = generatedDefault.DateOfBirth,
                IsPupilPremium = generatedDefault.IsPupilPremium,
                LocalAuthorityCode = generatedDefault.LocalAuthorityCode,
            };
            output.Add(outputPupil);
        });

        return PupilDtos.Create(output);
    }

    private static Faker<PupilDto> CreateGenerator()
    {
        Faker<PupilDto> faker = new();
        faker.StrictMode(true);
        faker.RuleFor(t => t.UniquePupilNumber, (f) => UniquePupilNumberTestDoubles.Generate().Value);
        faker.RuleFor(t => t.DateOfBirth, (f)
            => new DateOfBirth(
                DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(f.Random.Number(5, 18))).ToString());
        faker.RuleFor(t => t.Forename, (f) => f.Name.FirstName());
        faker.RuleFor(t => t.Surname, (f) => f.Name.LastName());
        faker.RuleFor(t => t.LocalAuthorityCode, (f) => f.Random.Number(0, 999));
        faker.RuleFor(t => t.Sex, (f) => f.PickRandom(Sex.Female.ToString(), Sex.Male.ToString()));
        faker.RuleFor(t => t.IsPupilPremium, (f) => f.Random.Bool());
        return faker;
    }
}
