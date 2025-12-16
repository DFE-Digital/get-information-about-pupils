using Bogus;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Features.MyPupils.DataTransferObjects;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.SharedTests.Features.MyPupils.Application;
public static class MyPupilModelTestDoubles
{
    public static MyPupilsModel Generate(int count)
    {
        return MyPupilsModel.Create(
            pupils: CreateGenerator().Generate(count));
    }

    public static MyPupilsModel GenerateWithUniquePupilNumbers(IEnumerable<UniquePupilNumber> uniquePupilNumbers)
    {
        Faker<MyPupilModel> generator = CreateGenerator();
        List<MyPupilModel> output = [];

        uniquePupilNumbers.ToList().ForEach(upn =>
        {
            MyPupilModel dto = MyPupilDtoBuilder.Create()
                .WithUniquePupilNumber(upn)
                .Build();

            output.Add(dto);
        });

        return MyPupilsModel.Create(pupils: output);
    }

    public static Faker<MyPupilModel> CreateGenerator()
    {
        Faker<MyPupilModel> faker = new();
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
