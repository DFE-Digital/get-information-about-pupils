using Bogus;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.MyPupils;
public static class MyPupilDtosTestDoubles
{
    public static MyPupilDto Generate() => Generate(count: 1).Values.Single();

    public static MyPupilDtos Generate(int count)
    {
        return MyPupilDtos.Create(
            pupils: CreateGenerator().Generate(count));
    }

    public static MyPupilDtos GenerateWithUniquePupilNumbers(IEnumerable<UniquePupilNumber> uniquePupilNumbers)
    {
        Faker<MyPupilDto> generator = CreateGenerator();
        List<MyPupilDto> output = [];

        uniquePupilNumbers.ToList().ForEach(upn =>
        {
            MyPupilDto generatedDefault = generator.Generate();
            MyPupilDto outputPupil = new()
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

        return MyPupilDtos.Create(pupils: output);
    }

    public static Faker<MyPupilDto> CreateGenerator()
    {
        Faker<MyPupilDto> faker = new();
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
