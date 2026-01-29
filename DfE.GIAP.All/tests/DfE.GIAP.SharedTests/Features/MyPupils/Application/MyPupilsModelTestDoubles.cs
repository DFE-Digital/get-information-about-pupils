using Bogus;
using DfE.GIAP.Core.Common.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Features.MyPupils.DataTransferObjects;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.SharedTests.Features.MyPupils.Application;
public static class MyPupilsModelTestDoubles
{
    public static MyPupilsModels Generate(int count)
    {
        return MyPupilsModels.Create(
            pupils: CreateGenerator().Generate(count));
    }

    public static MyPupilsModels GenerateWithUniquePupilNumbers(IEnumerable<UniquePupilNumber> uniquePupilNumbers)
    {
        Faker<MyPupilsModel> generator = CreateGenerator();
        List<MyPupilsModel> output = [];

        uniquePupilNumbers.ToList().ForEach(upn =>
        {
            MyPupilsModel dto = MyPupilDtoBuilder.Create()
                .WithUniquePupilNumber(upn)
                .Build();

            output.Add(dto);
        });

        return MyPupilsModels.Create(pupils: output);
    }

    public static Faker<MyPupilsModel> CreateGenerator()
    {
        Faker<MyPupilsModel> faker = new();
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
