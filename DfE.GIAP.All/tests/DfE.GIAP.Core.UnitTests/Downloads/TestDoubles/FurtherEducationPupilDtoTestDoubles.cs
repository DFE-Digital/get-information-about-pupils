using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class FurtherEducationPupilDtoTestDoubles
{
    public static List<FurtherEducationPupilDto> Generate(int count = 10, Func<FurtherEducationPupilDto, bool>? predicateToFulfil = null)
    {
        List<FurtherEducationPupilDto> pupils = [];

        Faker<FurtherEducationPupilDto> faker = CreateGenerator();

        const int circuitBreakerGenerationCounter = 111_111;

        for (int attempts = 0; pupils.Count < count && attempts < circuitBreakerGenerationCounter; attempts++)
        {
            FurtherEducationPupilDto dto = faker.Generate();
            if (predicateToFulfil is null || predicateToFulfil(dto))
            {
                pupils.Add(dto);
            }
        }

        if (pupils.Count < count)
        {
            throw new ArgumentException($"Unable to generate {count} DTOs after {circuitBreakerGenerationCounter} attempts.");
        }

        return pupils;
    }

    public static FurtherEducationPupilDto GenerateEmpty() => new()
    {
        UniqueLearnerNumber = null!,
        Forename = null!,
        Surname = null!,
        Gender = null!,
        DOB = default,
        ConcatenatedName = null!,
        PupilPremium = [],
        specialEducationalNeeds = []
    };

    private static Faker<FurtherEducationPupilDto> CreateGenerator()
    {
        Faker<PupilPremiumEntryDto> pupilPremiumFaker = new Faker<PupilPremiumEntryDto>()
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
            .RuleFor(p => p.Gender, f => f.PickRandom("Male", "Female", "Other"))
            .RuleFor(p => p.DOB, f => f.Date.Past(18, DateTime.Today.AddYears(-16)))
            .RuleFor(p => p.ConcatenatedName, (f, p) => $"{p.Surname}, {p.Forename}")
            .RuleFor(p => p.PupilPremium, f => pupilPremiumFaker.Generate(f.Random.Int(1, 3)))
            .RuleFor(p => p.specialEducationalNeeds, f => senFaker.Generate(f.Random.Int(1, 3)));
    }
}
