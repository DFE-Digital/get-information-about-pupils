using Bogus;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class PupilPremiumPupilDtoTestDoubles
{
    public static List<PupilPremiumPupilDto> Generate(
        int count = 10,
        Func<PupilPremiumPupilDto, bool>? predicate = null)
    {
        Faker<PupilPremiumPupilDto> faker = CreateGenerator();
        List<PupilPremiumPupilDto> results = new List<PupilPremiumPupilDto>();

        const int maxAttempts = 999;
        for (int i = 0; results.Count < count && i < maxAttempts; i++)
        {
            PupilPremiumPupilDto dto = faker.Generate();
            if (predicate?.Invoke(dto) != false)
            {
                results.Add(dto);
            }
        }

        if (results.Count < count)
        {
            throw new ArgumentException(
                $"Only generated {results.Count} of {count} requested DTOs after {maxAttempts} attempts.");
        }

        return results;
    }

    private static Faker<PupilPremiumPupilDto> CreateGenerator()
    {
        // Nested entry faker
        Faker<PupilPremiumEntryDto> entryFaker = new Faker<PupilPremiumEntryDto>()
            .StrictMode(true)
            .RuleFor(e => e.UniquePupilNumber, f => f.Random.Replace("##########"))
            .RuleFor(e => e.Surname, f => f.Name.LastName())
            .RuleFor(e => e.Forename, f => f.Name.FirstName())
            .RuleFor(e => e.Sex, (f, _) => f.PickRandom(new[] { "M", "F", "O" }))
            .RuleFor(e => e.DOB, f => f.Date.Past(18, DateTime.Today.AddYears(-16)).ToString("yyyy-MM-dd"))
            .RuleFor(e => e.NCYear, f => f.Random.Int(1, 13).ToString())
            .RuleFor(e => e.DeprivationPupilPremium, f => f.Random.Int(0, 1))
            .RuleFor(e => e.ServiceChildPremium, f => f.Random.Int(0, 1))
            .RuleFor(e => e.AdoptedfromCarePremium, f => f.Random.Int(0, 1))
            .RuleFor(e => e.LookedAfterPremium, f => f.Random.Int(0, 1))
            .RuleFor(e => e.PupilPremiumFTE, f => f.Random.Decimal(0.0m, 1.0m).ToString("0.00"))
            .RuleFor(e => e.PupilPremiumCashAmount, f => f.Random.Decimal(100m, 1000m).ToString("0"))
            .RuleFor(e => e.PupilPremiumFYStartDate, f => f.Date.Past(1).ToString("yyyy-MM-dd"))
            .RuleFor(e => e.PupilPremiumFYEndDate, (f, e) =>
                DateTime.Parse(e.PupilPremiumFYStartDate!).AddMonths(12).ToString("yyyy-MM-dd"))
            .RuleFor(e => e.LastFSM, f => f.Date.Past(2).ToString("yyyy-MM-dd"))
            .RuleFor(e => e.MODSERVICE, f => f.Random.Int(0, 1).ToString())
            .RuleFor(e => e.CENSUSSERVICEEVER6, f => f.Random.Int(0, 1).ToString());

        // Main DTO faker
        return new Faker<PupilPremiumPupilDto>()
            .StrictMode(true)
            .RuleFor(p => p.UniquePupilNumber, f => f.Random.Replace("##########"))
            .RuleFor(p => p.UniqueReferenceNumber, f => f.Random.Replace("########"))
            .RuleFor(p => p.Forename, f => f.Name.FirstName())
            .RuleFor(p => p.Surname, f => f.Name.LastName())
            .RuleFor(p => p.Sex, (f, _) => f.PickRandom(new[] { "M", "F", "O" }))
            .RuleFor(p => p.DOB, f => f.Date.Past(18, DateTime.Today.AddYears(-16)))
            .RuleFor(p => p.ConcatenatedName, (f, p) => $"{p.Surname}, {p.Forename}")
            .RuleFor(p => p.PupilPremium, f => entryFaker.Generate(f.Random.Int(1, 3)));
    }
}
