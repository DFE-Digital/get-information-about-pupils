using Bogus;
using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

internal static class FurtherEducationPupilTestDoubles
{
    internal static FurtherEducationPupil Create(bool includePupilPremium = false, bool includeSen = false)
    {
        Faker<PupilPremiumEntry> pupilPremiumFaker = new Faker<PupilPremiumEntry>()
            .StrictMode(true)
            .RuleFor(p => p.NationalCurriculumYear, f => f.Random.Int(1, 13).ToString())
            .RuleFor(p => p.FullTimeEquivalent, f => f.Random.Double(0.5, 1.0).ToString("0.0"))
            .RuleFor(p => p.AcademicYear, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}");

        Faker<SpecialEducationalNeedsEntry> senFaker = new Faker<SpecialEducationalNeedsEntry>()
            .StrictMode(true)
            .RuleFor(s => s.NationalCurriculumYear, f => f.Random.Int(1, 13).ToString())
            .RuleFor(s => s.Provision, f => f.PickRandom("SEN Support", "EHCP", "None"))
            .RuleFor(s => s.AcademicYear, f => $"{f.Date.Past(1).Year}/{f.Date.Past(1).Year + 1}");

        return new Faker<FurtherEducationPupil>()
            .UseSeed(13487123)
            .StrictMode(true)
            .RuleFor(p => p.UniqueLearnerNumber, f => f.Random.Replace("##########"))
            .RuleFor(p => p.Forename, f => f.Name.FirstName())
            .RuleFor(p => p.Surname, f => f.Name.LastName())
            .RuleFor(p => p.Gender, f => f.PickRandom("Male", "Female", "Other"))
            .RuleFor(p => p.DOB, f => f.Date.Past(18, DateTime.Today.AddYears(-16)))
            .RuleFor(p => p.ConcatenatedName, (f, p) => $"{p.Surname}, {p.Forename}")
            .RuleFor(p => p.PupilPremium, f => includePupilPremium ? pupilPremiumFaker.Generate(f.Random.Int(1, 3)) : new List<PupilPremiumEntry>())
            .RuleFor(p => p.specialEducationalNeeds, f => includeSen ? senFaker.Generate(f.Random.Int(1, 2)) : new List<SpecialEducationalNeedsEntry>())
            .Generate();
    }
}
