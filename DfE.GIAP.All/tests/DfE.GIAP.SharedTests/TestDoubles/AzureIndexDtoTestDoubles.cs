using Bogus;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using Fare;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class AzureIndexDtosTestDoubles
{
    public static List<AzureIndexEntity> Generate(int count = 10)
    {
        return CreateFaker().Generate(count);
    }


    public static Faker<AzureIndexEntity> CreateFaker()
    {

        // To generate based on a Upn validator regex
        Xeger xeger = new(
            UniquePupilNumberValidator.UpnRegex().ToString(),
            new Random());

        Faker<AzureIndexEntity> faker = new Faker<AzureIndexEntity>()
            .RuleFor(t => t.Score, f => f.Random.Double(0, 1).ToString("F2"))
            .RuleFor(t => t.UPN, _ => xeger.Generate())
            .RuleFor(t => t.Forename, f => f.Name.FirstName())
            .RuleFor(t => t.Surname, f => f.Name.LastName())
            .RuleFor(t => t.Sex, f =>
            {
                char[] validSexs = [Sex.Male.ToString()[0], Sex.Female.ToString()[0]];
                return f.PickRandom(validSexs);
            })
            .RuleFor(t => t.DOB, f => DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(f.Random.Number(1, 25)))
            .RuleFor(t => t.LocalAuthority, f => f.Random.Number(100, 999).ToString())
            .RuleFor(t => t.id, f => f.Random.Guid().ToString());
        return faker;
    }
}
