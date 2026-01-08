using Bogus;
using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

internal static class NationalPupilTestDoubles
{
    internal static NationalPupil Create(
        bool includeKeyStages = false,
        bool includeCensus = false,
        bool includeMtc = false,
        bool includePhonics = false,
        bool includeEyfsp = false)
    {
        Faker<KeyStage1Entry> keyStage1Faker = new Faker<KeyStage1Entry>()
            .StrictMode(false)
            .RuleFor(k => k.ACADYR, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}")
            .RuleFor(k => k.UPN, f => f.Random.Replace("########"))
            .RuleFor(k => k.SURNAME, f => f.Name.LastName());

        Faker<KeyStage2Entry> keyStage2Faker = new Faker<KeyStage2Entry>()
            .StrictMode(false)
            .RuleFor(k => k.ACADYR, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}")
            .RuleFor(k => k.UPN, f => f.Random.Replace("########"))
            .RuleFor(k => k.SURNAME, f => f.Name.LastName());

        Faker<KeyStage4Entry> keyStage4Faker = new Faker<KeyStage4Entry>()
            .StrictMode(false)
            .RuleFor(k => k.ACADYR, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}")
            .RuleFor(k => k.UPN, f => f.Random.Replace("########"))
            .RuleFor(k => k.SURNAME, f => f.Name.LastName());

        Faker<MtcEntry> mtcFaker = new Faker<MtcEntry>()
            .StrictMode(false)
            .RuleFor(m => m.ACADYR, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}")
            .RuleFor(m => m.UPN, f => f.Random.Replace("########"))
            .RuleFor(m => m.Surname, f => f.Name.LastName());

        Faker<CensusAutumnEntry> censusAutumnFaker = new Faker<CensusAutumnEntry>()
            .StrictMode(false)
            .RuleFor(c => c.UniquePupilNumber, f => f.Random.Replace("########"))
            .RuleFor(c => c.Surname, f => f.Name.LastName())
            .RuleFor(c => c.AcademicYear, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}");

        Faker<CensusSpringEntry> censusSpringFaker = new Faker<CensusSpringEntry>()
            .StrictMode(false)
            .RuleFor(c => c.UniquePupilNumber, f => f.Random.Replace("########"))
            .RuleFor(c => c.Surname, f => f.Name.LastName())
            .RuleFor(c => c.AcademicYear, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}");

        Faker<CensusSummerEntry> censusSummerFaker = new Faker<CensusSummerEntry>()
            .StrictMode(false)
            .RuleFor(c => c.UniquePupilNumber, f => f.Random.Replace("########"))
            .RuleFor(c => c.Surname, f => f.Name.LastName())
            .RuleFor(c => c.AcademicYear, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}");

        Faker<EarlyYearsFoundationStageProfileEntry> eyfspFaker = new Faker<EarlyYearsFoundationStageProfileEntry>()
            .StrictMode(false)
            .RuleFor(e => e.UPN, f => f.Random.Replace("########"))
            .RuleFor(e => e.SURNAME, f => f.Name.LastName())
            .RuleFor(e => e.ACADYR, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}");

        Faker<PhonicsEntry> phonicsFaker = new Faker<PhonicsEntry>()
            .StrictMode(false)
            .RuleFor(p => p.UniquePupilNumber, f => f.Random.Replace("########"))
            .RuleFor(p => p.SurName, f => f.Name.LastName())
            .RuleFor(p => p.AcademicYear, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}")
            .RuleFor(p => p.Phonics_Mark, f => f.Random.Int(0, 40).ToString());

        return new Faker<NationalPupil>()
            .UseSeed(13487123)
            .StrictMode(false)
            .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
            .RuleFor(p => p.PupilMatchingRef, f => f.Random.Replace("########"))
            .RuleFor(p => p.Upn, f => f.Random.Replace("########"))
            .RuleFor(p => p.Surname, f => f.Name.LastName())
            .RuleFor(p => p.Forename, f => f.Name.FirstName())
            .RuleFor(p => p.MiddleName, f => f.Name.FirstName())
            .RuleFor(p => p.Gender, f => f.PickRandom('M', 'F'))
            .RuleFor(p => p.Sex, f => f.PickRandom('M', 'F'))
            .RuleFor(p => p.DOB, f => f.Date.Past(16, DateTime.Today.AddYears(-10)))
            .RuleFor(p => p.LA, f => f.Random.Int(200, 999))
            .RuleFor(p => p.Estab, f => f.Random.Int(10000, 99999))
            .RuleFor(p => p.Urn, f => f.Random.Int(100000, 999999))
            .RuleFor(p => p.KeyStage1, f => includeKeyStages ? keyStage1Faker.Generate(f.Random.Int(1, 2)) : new())
            .RuleFor(p => p.KeyStage2, f => includeKeyStages ? keyStage2Faker.Generate(f.Random.Int(1, 2)) : new())
            .RuleFor(p => p.KeyStage4, f => includeKeyStages ? keyStage4Faker.Generate(f.Random.Int(1, 2)) : new())
            .RuleFor(p => p.CensusAutumn, f => includeCensus ? censusAutumnFaker.Generate(f.Random.Int(1, 2)) : new())
            .RuleFor(p => p.CensusSpring, f => includeCensus ? censusSpringFaker.Generate(f.Random.Int(1, 2)) : new())
            .RuleFor(p => p.CensusSummer, f => includeCensus ? censusSummerFaker.Generate(f.Random.Int(1, 2)) : new())
            .RuleFor(p => p.MTC, f => includeMtc ? mtcFaker.Generate(f.Random.Int(1, 2)) : new())
            .RuleFor(p => p.EarlyYearsFoundationStageProfile, f => includeEyfsp ? eyfspFaker.Generate(f.Random.Int(1, 2)) : new())
            .RuleFor(p => p.Phonics, f => includePhonics ? phonicsFaker.Generate(f.Random.Int(1, 2)) : new())
            .Generate();
    }
}
