using Bogus;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class FurtherEducationPupilDtoTestDoubles
{
    public static List<FurtherEducationPupilDto> Generate(int count = 10, Func<FurtherEducationPupilDto, bool>? predicate = null)
    {
        Faker<FurtherEducationPupilDto> faker = CreateGenerator();
        List<FurtherEducationPupilDto> pupils = new List<FurtherEducationPupilDto>();

        const int maxAttempts = 999;
        for (int i = 0; pupils.Count < count && i < maxAttempts; i++)
        {
            FurtherEducationPupilDto dto = faker.Generate();
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


    private static Faker<FurtherEducationPupilDto> CreateGenerator()
    {
        Faker<FurtherEducationPupilPremiumEntryDto> pupilPremiumFaker = new Faker<FurtherEducationPupilPremiumEntryDto>()
            .StrictMode(true)
            .RuleFor(p => p.NationalCurriculumYear, f => f.Random.Int(1, 13).ToString())
            .RuleFor(p => p.FullTimeEquivalent, f => f.Random.Double(0.5, 1.0).ToString("0.0"))
            .RuleFor(p => p.AcademicYear, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}");

        Faker<SpecialEducationalNeedsEntryDto> senFaker = new Faker<SpecialEducationalNeedsEntryDto>()
            .StrictMode(true)
            .RuleFor(s => s.NationalCurriculumYear, f => f.Random.Int(1, 13).ToString())
            .RuleFor(s => s.Provision, f => f.PickRandom("SEN Support", "EHCP", "None"))
            .RuleFor(s => s.AcademicYear, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}");

        return new Faker<FurtherEducationPupilDto>()
            .StrictMode(true)
            .RuleFor(p => p.UniqueLearnerNumber, f => f.Random.Replace("##########"))
            .RuleFor(p => p.Forename, f => f.Name.FirstName())
            .RuleFor(p => p.Surname, f => f.Name.LastName())
            .RuleFor(p => p.Sex, f => f.PickRandom("Male", "Female", "Other"))
            .RuleFor(p => p.DOB, f => f.Date.Past(18, DateTime.Today.AddYears(-16)))
            .RuleFor(p => p.ConcatenatedName, (f, p) => $"{p.Surname}, {p.Forename}")
            .RuleFor(p => p.PupilPremium, f => pupilPremiumFaker.Generate(f.Random.Int(1, 3)))
            .RuleFor(p => p.specialEducationalNeeds, f => senFaker.Generate(f.Random.Int(1, 3)));
    }
}
