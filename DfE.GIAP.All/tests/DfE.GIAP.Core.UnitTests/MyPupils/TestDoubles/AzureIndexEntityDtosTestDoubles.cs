using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
public static class AzureIndexEntityDtosTestDoubles
{
    public static List<AzureIndexEntity> Generate(int count = 10)
    {
        return CreateFaker().Generate(count);
    }

    public static List<AzureIndexEntity> GenerateWithUpns(IEnumerable<UniquePupilNumber> upns)
    {
        List<AzureIndexEntity> generated = CreateFaker().Generate(upns.Count());

        int index = 0;
        upns.ToList().ForEach(t =>
        {
            generated[index].UPN = t.Value;
            index++;
        });

        return generated;
    }

    private static Faker<AzureIndexEntity> CreateFaker()
    {
        Faker<AzureIndexEntity> faker = new Faker<AzureIndexEntity>()
            .RuleFor(t => t.Score, f => f.Random.Double(0, 1).ToString("F2"))
            .RuleFor(t => t.UPN, _ => UniquePupilNumberTestDoubles.Generate().Value)
            .RuleFor(t => t.Forename, f => f.Name.FirstName())
            .RuleFor(t => t.Surname, f => f.Name.LastName())
            .RuleFor(t => t.Sex, f =>
            {
                string[] validSexs = [Sex.Male.ToString(), Sex.Female.ToString()];
                return f.PickRandom(validSexs);
            })
            .RuleFor(t => t.Gender, f =>
            {
                string[] validSexs = [Sex.Male.ToString(), Sex.Female.ToString()];
                return f.PickRandom(validSexs);
            })
            .RuleFor(t => t.DOB, f => DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(f.Random.Number(1, 25)))
            .RuleFor(t => t.LocalAuthority, f => f.Random.Number(100, 999).ToString())
            .RuleFor(t => t.id, f => f.Random.Guid().ToString());
        return faker;
    }
}
