using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
internal static class MyPupilsPresentationPupilModelsTestDoubles
{
    internal static MyPupilsPresentationPupilModels Generate(int count)
    {
        return MyPupilsPresentationPupilModels.Create(
            pupils: GeneratePupils(count));
    }

    private static List<MyPupilsPresentationPupilModel> GeneratePupils(int count)
    {
        return CreateGenerator().Generate(count);
    }

    internal static Faker<MyPupilsPresentationPupilModel> CreateGenerator()
    {
        Faker<MyPupilsPresentationPupilModel> faker = new();
        faker.StrictMode(true);
        faker.RuleFor(t => t.UniquePupilNumber, (f) => UniquePupilNumberTestDoubles.Generate().Value);
        faker.RuleFor(t => t.DateOfBirth, (f)
            => new DateOfBirth(
                DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(f.Random.Number(5, 18))).ToString());
        faker.RuleFor(t => t.Forename, (f) => f.Name.FirstName());
        faker.RuleFor(t => t.Surname, (f) => f.Name.LastName());
        faker.RuleFor(t => t.LocalAuthorityCode, (f) => f.Random.Number(0, 999).ToString());
        faker.RuleFor(t => t.Sex, (f) => f.PickRandom(Sex.Female.ToString(), Sex.Male.ToString()));
        faker.RuleFor(t => t.PupilPremiumLabel, (f) => f.PickRandom("Yes", "No"));
        faker.RuleFor(t => t.IsSelected, (f) => f.Random.Bool());
        return faker;
    }
}
