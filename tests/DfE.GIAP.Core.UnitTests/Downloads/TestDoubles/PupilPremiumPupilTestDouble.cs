using Bogus;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

internal static class PupilPremiumPupilTestDouble
{
    internal static PupilPremiumPupil Create(bool includePupilPremium = false)
    {
        Faker<PupilPremiumEntry> pupilPremiumEntryFaker = new Faker<PupilPremiumEntry>()
            .StrictMode(true)
            .RuleFor(p => p.UniquePupilNumber, f => f.Random.Replace("##########"))
            .RuleFor(p => p.Surname, f => f.Name.LastName())
            .RuleFor(p => p.Forename, f => f.Name.FirstName())
            .RuleFor(p => p.Sex, f => f.PickRandom("Male", "Female", "Other"))
            .RuleFor(p => p.DOB, f => DateTime.Parse(f.Date.Past(18, DateTime.Today.AddYears(-16)).ToString("yyyy-MM-dd")))
            .RuleFor(p => p.NCYear, f => f.Random.Int(1, 13).ToString())
            .RuleFor(p => p.DeprivationPupilPremium, f => f.Random.Int(0, 1))
            .RuleFor(p => p.ServiceChildPremium, f => f.Random.Int(0, 1))
            .RuleFor(p => p.AdoptedfromCarePremium, f => f.Random.Int(0, 1))
            .RuleFor(p => p.LookedAfterPremium, f => f.Random.Int(0, 1))
            .RuleFor(p => p.PupilPremiumFTE, f => f.Random.Double(0.5, 1.0).ToString("0.0"))
            .RuleFor(p => p.PupilPremiumCashAmount, f => f.Random.Int(100, 3000).ToString())
            .RuleFor(p => p.PupilPremiumFYStartDate, f => f.Date.Past(1).ToString("yyyy-MM-dd"))
            .RuleFor(p => p.PupilPremiumFYEndDate, (f, p) => f.Date.Future(1, DateTime.Parse(p.PupilPremiumFYStartDate!)).ToString("yyyy-MM-dd"))
            .RuleFor(p => p.LastFSM, f => f.PickRandom("Y", "N"))
            .RuleFor(p => p.MODSERVICE, f => f.PickRandom("Y", "N"))
            .RuleFor(p => p.CENSUSSERVICEEVER6, f => f.PickRandom("Y", "N"));

        return new Faker<PupilPremiumPupil>()
            .UseSeed(987654321)
            .StrictMode(true)
            .RuleFor(p => p.UniquePupilNumber, f => f.Random.Replace("##########"))
            .RuleFor(p => p.UniqueReferenceNumber, f => f.Random.Replace("########"))
            .RuleFor(p => p.Forename, f => f.Name.FirstName())
            .RuleFor(p => p.Surname, f => f.Name.LastName())
            .RuleFor(p => p.Sex, f => f.PickRandom("Male", "Female", "Other"))
            .RuleFor(p => p.DOB, f => f.Date.Past(18, DateTime.Today.AddYears(-16)))
            .RuleFor(p => p.ConcatenatedName, (f, p) => $"{p.Surname}, {p.Forename}")
            .RuleFor(p => p.PupilPremium, f => includePupilPremium
                ? pupilPremiumEntryFaker.Generate(f.Random.Int(1, 3))
                : new List<PupilPremiumEntry>())
            .Generate();
    }
}
