using Bogus;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects.Entries;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class NationalPupilDtoTestDoubles
{
    public static List<NationalPupilDto> Generate(int count = 10, Func<NationalPupilDto, bool>? predicate = null)
    {
        Faker<NationalPupilDto> faker = CreateGenerator();
        List<NationalPupilDto> pupils = new List<NationalPupilDto>();

        const int maxAttempts = 999;
        for (int i = 0; pupils.Count < count && i < maxAttempts; i++)
        {
            NationalPupilDto dto = faker.Generate();
            if (predicate?.Invoke(dto) != false)
            {
                pupils.Add(dto);
            }
        }

        if (pupils.Count < count)
        {
            throw new ArgumentException($"Only generated {pupils.Count} of {count} requested DTOs after {maxAttempts} attempts.");
        }

        return pupils;
    }

    private static Faker<NationalPupilDto> CreateGenerator()
    {
        return new Faker<NationalPupilDto>()
            .StrictMode(true)
            .RuleFor(p => p.Upn, f => f.Random.Replace("########"))
            .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
            .RuleFor(p => p.PupilMatchingRef, f => f.Random.Replace("########"))
            .RuleFor(p => p.LA, f => f.Random.Int(200, 999))
            .RuleFor(p => p.Estab, f => f.Random.Int(10000, 99999))
            .RuleFor(p => p.Urn, f => f.Random.Int(100000, 999999))
            .RuleFor(p => p.Surname, f => f.Name.LastName())
            .RuleFor(p => p.Forename, f => f.Name.FirstName())
            .RuleFor(p => p.MiddleName, f => f.Name.FirstName())
            .RuleFor(p => p.Gender, f => f.PickRandom('M', 'F'))
            .RuleFor(p => p.Sex, f => f.PickRandom('M', 'F'))
            .RuleFor(p => p.DOB, f => f.Date.Past(16, DateTime.Today.AddYears(-10)))
            .RuleFor(p => p.CensusAutumn, f => new List<CensusAutumnEntryDto>())
            .RuleFor(p => p.CensusSpring, f => new List<CensusSpringEntryDto>())
            .RuleFor(p => p.CensusSummer, f => new List<CensusSummerEntryDto>())
            .RuleFor(p => p.EarlyYearsFoundationStageProfile, f => new List<EarlyYearsFoundationStageProfileEntryDto>())
            .RuleFor(p => p.Phonics, f => new List<PhonicsEntryDto>())
            .RuleFor(p => p.KeyStage1, f => new List<KeyStage1EntryDto>())
            .RuleFor(p => p.KeyStage2, f => new List<KeyStage2EntryDto>())
            .RuleFor(p => p.KeyStage4, f => new List<KeyStage4EntryDto>())
            .RuleFor(p => p.MTC, f => new List<MtcEntryDto>());
    }
}
